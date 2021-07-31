
This example demonstrate how a nodejs / typescript application could consume (call) the *OrderService* via RabbitMQ

The caller application will have to know

* The name of the exchange in RabbitMQ.
* The input / output types and structures.


Our nodejs application wants to call the *NewOrder* function of the *OrderService*.

Take a look at the contract of this function in the service documentation (swagger in our example):

![NewOrder function contract](https://user-images.githubusercontent.com/1638594/127748110-9876f77d-1504-4be3-b9b1-9277837f6e80.png)

we know that the input type is `OrderService.API.Models.NewOrderCommand` so
* the exchange we will have to use is `OrderService.API.Models:NewOrderCommand`, it is a convention
* the RequestType is `new MessageType("NewOrderCommand", "OrderService.API.Models")`
* the ResponseType is `new MessageType("Order", "OrderService.API.Models")`

* the input structure can be described in typescript as following

```ts
interface NewOrderCommand {
    statusCode: number;
    statusText: string;
}
```

* the output structure can be described in typescript as following

```ts
interface Order {
    id: string;
    statusCode: number;
    statusMessage: string;
    dateCreated: Date;
}
```

We have all the information needed to create the Client:

```ts
const client = bus.requestClient<NewOrderCommand, Order>({
    exchange: "OrderService.API.Models:NewOrderCommand",
    requestType: new MessageType("NewOrderCommand", messageNamespace),
    responseType: new MessageType("Order", messageNamespace),
});
```

then use it to send the Request and get the Response:

```ts
const response = await client.getResponse({
        statusCode: 2,
        statusText: "toto",
    });
```

Note: if the *OrderService* is not started (offline) then the call will hang until the service is available again.

# How to run

```
npm start
```

# This example is not answered the following questions

* How to set a timeout so that the `await client.getResponse` will throw an error after 30sec (for example).
[Here is a potential answer](https://www.npmjs.com/package/await-timeout)

* How to declare the login / password to connect to the RabbitMQ



