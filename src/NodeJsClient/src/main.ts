import { MessageType } from "masstransit-rabbitmq/dist/messageType";
import masstransit from "masstransit-rabbitmq";

const messageNamespace = "OrderService.API.Models";

const bus = masstransit({
    host: "localhost",
    virtualHost: "/",
    port: 5672,
});

interface NewOrderCommand {
    statusCode: number;
    statusText: string;
}
interface Order {
    id: string;
    statusCode: number;
    statusMessage: string;
    dateCreated: Date;
}

const client = bus.requestClient<NewOrderCommand, Order>({
    exchange: messageNamespace + ":NewOrderCommand",
    requestType: new MessageType("NewOrderCommand", messageNamespace),
    responseType: new MessageType("Order", messageNamespace),
});

async function main() {
    try  {
        const response = await client.getResponse({
            statusCode: -2,
            statusText: "toto",
        });
        console.info(response.message);
    }
    catch (e) {
        console.error("Failed to submit order", e.message)
    }
    await bus.stop();
}

(async () => {
    try {
        await main();
        console.log("Finished OK");
    } catch (e) {
        console.log("Failed ", e);
    }
})();
