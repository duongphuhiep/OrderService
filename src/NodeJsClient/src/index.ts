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
//const DeserializersCache: Record<string, deseri

class Bus {
    waitingRequestsCache: Record<string, WaitingRequest> = {};

    connectOptions: amqp.Options.Connect;
    connection: amqp.Connection;
    channel: amqp.ConfirmChannel;
    queueName: string;
    exchange: amqp.Replies.AssertExchange;
    queue: amqp.Replies.AssertQueue;
    consumer: amqp.Replies.Consume;
    
    constructor(connectOptions: amqp.Options.Connect) {
        this.connectOptions = connectOptions;
    }
    
    /**
     * The bus will auto-deleted after 60s inactivity
     * @param expiry 
     */
    public async start() {
        this.waitingRequestsCache = {}; //clear the cache
        this.connection = await amqp.connect(this.connectOptions);
        this.connection.on('close', () => {
            console.log("connection closed");
            //TODO: scheduleReconnect(); rebuild the Bus
        });
        process.on('SIGINT', async () => {
            await this.stop();
        });
        
        this.channel = await this.connection.createConfirmChannel();
        this.channel.prefetch(1, true)
        //await channel.prefetch(this.options.prefetchCount, this.options.globalPrefetch);
        
        this.queueName = `bus-${nanoid()}`;
        
        this.exchange = await this.channel.assertExchange(this.queueName, 'fanout', {
            durable: false, 
            autoDelete: true, 
            //arguments: {'x-expires': expiry},
        });
        this.queue = await this.channel.assertQueue(this.queueName, {
            durable: false, 
            autoDelete: true, 
            //arguments: {'x-expires': expiry},
        });
        
        await this.channel.bindQueue(this.queueName, this.queueName, '');
        console.log('Queue:', this.queue.queue, 'MessageCount:', this.queue.messageCount, 'ConsumerCount:', this.queue.consumerCount);

        this.consumer = await this.channel.consume(this.queueName, msg => {
            const wrappedResponseText = msg.content.toString();
            const wrappedResponse = <WrappedResponse<object>>deserialize(WrappedResponse, wrappedResponseText);
            const requestId = wrappedResponse?.requestId;
            if (requestId) {
                const waitingRequest = this.waitingRequestsCache[requestId];
                if (waitingRequest) {
                    waitingRequest.responseConsumer.resolveResponse(wrappedResponse);
                    clearTimeout(waitingRequest.ontimeout);
                    delete this.waitingRequestsCache[requestId]
                    this.channel.ack(msg, false);
                }
                else {
                    console.log(`requestId ${requestId} not found`)
                    this.channel.nack(msg, false, false);
                }
            }
            else {
                console.log(`Missing requestId in the response ${wrappedResponseText}`);
                this.channel.reject(msg, false);
            }
        });
        console.log('consumerTag=', this.consumer.consumerTag);
    }
    public async stop() {
        console.log("stop bus.");
        await this.connection.close();
    }
    
    public async getResponse<TRequest extends object, TResponse extends object>(exchangeName: string, wrappedRequest: WrappedRequest<TRequest>): Promise<WrappedResponse<TResponse>> {
        const responseConsumer = new ResponseConsumer<TResponse>();
        wrappedRequest.requestId = uuidv4();
        
        const waitingRequest: WaitingRequest = {
            requestId: wrappedRequest.requestId,
            responseConsumer,
        }
        waitingRequest.ontimeout = setTimeout(function(args){
            //if this ever gets called we didn't get a response in a timely fashion
            args.waitingRequest.responseConsumer.rejectResponse(new Error("timeout " + args.waitingRequest.requestId));
            //delete the entry from cache
            delete args.self.waitingRequestsCache[args.waitingRequest.requestId];
          }, 1000, {waitingRequest, self: this});
        this.waitingRequestsCache[wrappedRequest.requestId] = waitingRequest;
        
        await this.channel.publish(
            exchangeName,
            "",
            Buffer.from(serialize(wrappedRequest)),
            {persistent: true}
        );
        
        return responseConsumer.futureResponse;
    }
    
    public getResponseUrl(): URL {
        if (!this.queueName)
            throw Error('Bus is not started');
        const protocol = (this.connectOptions.protocol || 'amqp') + ':';
        let url = new URL(`${protocol}//127.0.0.1`);
        url.protocol = protocol;
        url.host = this.connectOptions.hostname;
        url.port = this.connectOptions.port ? this.connectOptions.port.toString(): null;
        url.username = this.connectOptions.username || '';
        url.password = this.connectOptions.password || '';
        url.pathname = this.connectOptions.vhost || '';
        url.pathname = url.pathname.endsWith('/') ? url.pathname + this.queueName : url.pathname + '/' + this.queueName;
        url.searchParams.append("temporary", "true")
        return url;
    }
}


async function main() {
    try {
        const bus = new Bus({
            hostname: 'localhost',
            vhost:'/',
            protocol:'amqp',
            heartbeat:60
        });
        await bus.start();
        try {
            const wrappedRequest: WrappedRequest<NewOrderCommand> = {
                requestId: null,
                responseAddress: bus.getResponseUrl().toString(),
                messageType: ["urn:message:OrderService.API.Models:NewOrderCommand"],
                message: {
                    statusCode: 2,
                    statusText: "foo",
                }
            };
            console.log("ResponseAddress = ", wrappedRequest.responseAddress);
            const response = await bus.getResponse<NewOrderCommand, Order>("OrderService.API.Models:NewOrderCommand", wrappedRequest);
            console.log('Got response', response);
        }
        finally {
            await bus.stop();
        }
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
