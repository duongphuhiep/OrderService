import amqp from "amqplib";
import {serialize, deserialize} from 'class-transformer';
import { nanoid } from "nanoid";
import { v4 as uuidv4 } from 'uuid';

const messageNamespace = "OrderService.API.Models";
class NewOrderCommand {
    statusCode: number;
    statusText: string;
}
class Order {
    id: string;
    statusCode: number;
    statusMessage: string;
    dateCreated: Date;
}

export interface FaultMessage {
    faultId: string;
    faultedMessageId: string;
    timestamp: Date;
    exceptions: any;
    faultMessageTypes: string;
    message: any;
}

export class WrappedResponse<TResponse extends object>{
    messageId?: string;
    requestId?: string;
    correlationId?: string;
    conversationId?: string;
    initiatorId?: string;
    expirationTime?: string;
    sourceAddress?: string;
    destinationAddress?: string;
    responseAddress?: string;
    faultAddress?: string;
    sentTime?: string;
    messageType?: Array<string>;
    headers?: object;
    host?: any;
    message!: TResponse|FaultMessage;
}

export class WrappedRequest<TRequest extends object> {
    requestId: string;
    responseAddress: string;
    messageType: string[];
    message: TRequest
}

interface Bus {
    connection: amqp.Connection;
    channel: amqp.ConfirmChannel;
    queueName: string;
    exchange: amqp.Replies.AssertExchange;
    queue: amqp.Replies.AssertQueue;
}

class ResponseConsumer<TResponse extends object> {
    futureResponse: Promise<WrappedResponse<TResponse>>;
    resolveResponse: (value: WrappedResponse<TResponse> | PromiseLike<WrappedResponse<TResponse>>) => void;
    rejectResponse: (reason?: any) => void;
    
    constructor() {
        this.futureResponse = new Promise<WrappedResponse<TResponse>>((resolve, reject) => {
            this.resolveResponse = resolve;
            this.rejectResponse = reject
        })
    }
}

interface WaitingRequest {
    requestId: string;
    ontimeout?: NodeJS.Timeout;
    responseConsumer: ResponseConsumer<object>;
    
}
const WaitingRequestsCache: Record<string, WaitingRequest> = {};
//const DeserializersCache: Record<string, deseri

async function startBus(): Promise<Bus> {
    const connection = await amqp.connect('amqp://localhost/?heartbeat=60');
    connection.on('close', () => {
        console.log("connection closed");
        //TODO: scheduleReconnect(); rebuild the Bus
    });
    process.on('SIGINT', async () => {
        await connection.close();
    });
    
    const channel = await connection.createConfirmChannel();
    channel.prefetch(1, true)
    //await channel.prefetch(this.options.prefetchCount, this.options.globalPrefetch);
    
    const queueName = `bus-${nanoid()}`;
    
    const exchange = await channel.assertExchange(queueName, 'fanout', {
        durable: false, 
        autoDelete: true, 
        arguments: {'x-expires': 60000},
    });
    const queue = await channel.assertQueue(queueName, {
        durable: false, 
        autoDelete: true, 
        arguments: {'x-expires': 60000},
    });
    
    await channel.bindQueue(queueName, queueName, '');
    console.log('Queue:', queue.queue, 'MessageCount:', queue.messageCount, 'ConsumerCount:', queue.consumerCount);

    const consumer = await channel.consume(queueName, msg => {
        const wrappedResponseText = msg.content.toString();
        const wrappedResponse = <WrappedResponse<object>>deserialize(WrappedResponse, wrappedResponseText);
        const requestId = wrappedResponse?.requestId;
        if (requestId) {
            const waitingRequest = WaitingRequestsCache[requestId];
            if (waitingRequest) {
                waitingRequest.responseConsumer.resolveResponse(wrappedResponse);
                clearTimeout(waitingRequest.ontimeout);
                delete WaitingRequestsCache[requestId]
                channel.ack(msg, false);
            }
            else {
                console.log(`requestId ${requestId} not found`)
                channel.nack(msg, false, false);
            }
        }
        else {
            console.log(`Missing requestId in the response ${wrappedResponseText}`);
            channel.reject(msg, false);
        }
    });
    console.log('consumerTag=', consumer.consumerTag);
    
    return {
        connection,
        channel,
        queueName,
        exchange,
        queue
    };
}

class RequestPublisher<TRequest extends object, TResponse extends object> {
    bus: Bus;
    constructor(bus: Bus) {
        this.bus = bus
    }
    public async getResponse(exchangeName: string, wrappedRequest: WrappedRequest<TRequest>): Promise<WrappedResponse<TResponse>> {
        const responseConsumer = new ResponseConsumer<TResponse>();
        wrappedRequest.requestId = uuidv4();
        
        const waitingRequest: WaitingRequest = {
            requestId: wrappedRequest.requestId,
            responseConsumer,
        }
        waitingRequest.ontimeout = setTimeout(function(wr: WaitingRequest){
            //if this ever gets called we didn't get a response in a timely fashion
            wr.responseConsumer.rejectResponse(new Error("timeout " + wr.requestId));
            //delete the entry from cache
            delete WaitingRequestsCache[wr.requestId];
          }, 1000, waitingRequest);
        WaitingRequestsCache[wrappedRequest.requestId] = waitingRequest;
        
        await this.bus.channel.publish(
            exchangeName,
            "",
            Buffer.from(serialize(wrappedRequest)),
            {persistent: true}
        );
        
        return responseConsumer.futureResponse;
    }
}

async function main() {
    try {
        const bus = await startBus();
        
        const wrappedRequest: WrappedRequest<NewOrderCommand> = {
            requestId: null,
            responseAddress: `amqp://localhost/${bus.queueName}?temporary=true`,
            messageType: ["urn:message:OrderService.API.Models:NewOrderCommand"],
            message: {
                statusCode: 2,
                statusText: "foo",
            }
        };
        const requestPublisher = new RequestPublisher<NewOrderCommand, Order>(bus);
        const response = await requestPublisher.getResponse("OrderService.API.Models:NewOrderCommand", wrappedRequest);
        
        console.log('Got response', response);
        
        await bus.connection.close();
    }
    catch (e) {
        console.error("Failed", e.message)
    }
}

(async () => {
    try {
        await main();
        console.log("Finished OK");
    } catch (e) {
        console.log("Failed ", e);
    }
})();
