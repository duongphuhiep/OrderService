This project is a fictif *OrderService* which exposes 2 functions:

* *NewOrder*: Create a new order (and save it to a volatile in-memory database)
* *GetOrder*: Get order information from its *id*

# How to play

## Step 1) Start rabbitmq on localhost

```
docker-compose -f docker-compose-infra.yml up
```
The RabbitMQ UI is accessible at
```
http://localhost:15672/
Login: guest
Password: guest
```

## Step 2) Run the *OrderService* on localhost

You can Run/Debug the project `OrderService.API` from Visual Studio or use command line

```
cd OrderService.API
dotnet run
```

You should be able to see the OrderService up and running at `https://localhost:5001/swagger`

## Step 3) Call (or consume) the service

The following instruction will help you call the *NewOrder* function from our *OrderService*

### Method 1: Use Postman (or Swagger)

This is the simplest method to call the service and get the response.

![Call service with Postman](https://user-images.githubusercontent.com/1638594/127404837-7d4a8d35-eb7d-44ba-b322-bfea38ae8ce5.png)

Behind the scene, when `OrderController` received this (postman) request, it publish on RabbitMQ to execute the *NewOrder*. It will also return the response of our micro-service from RabbitMQ.

### Method 2: Use directly the RabbitMQ UI

![Call service with rabbitmq UI](https://user-images.githubusercontent.com/1638594/130364739-28ca4682-8d9e-4cd4-abd7-2762a817e582.png)

When the `OrderService.API` application is started, it would create the exchange `OrderService.Contracts:NewOrderCommand`

You can publish the following message to this exchange in order to execute the *NewOrder* function
```
{
  "messageType": [
    "urn:message:OrderService.Contracts:NewOrderCommand"
  ],
  "message": {
    "statusCode": 4,
    "statusText": "4"
  }
}
```

#### How to see the response?

Unfortunately there is no easy way to see the response of the *NewOrder* function from the RabbitMQ dashboard. If you really want to see the response here what you can do:

1) Create a (fan-out) `debug_exchange` and a `debug_queue`, bind the `debug_exchange` to the `debug_queue`.

![debug exchange and queue](https://user-images.githubusercontent.com/1638594/127785037-e95dea18-e74e-4a82-bdf8-c47f569e6c0b.png)

2) when you publish the request, you can ask our micro-service to publish the response to the `debug_exchange`:

```
{
  "responseAddress": "rabbitmq://localhost/debug_exchange",
  "messageType": [
    "urn:message:OrderService.Contracts:NewOrderCommand"
  ],
  "message": {
    "statusCode": 4,
    "statusText": "4"
  }
}
```

3) Now you can go to the `debug_queue` and get the response message

![get response in debug_queue](https://user-images.githubusercontent.com/1638594/127785382-b62e95dd-b1dd-44ef-9366-fe33f6f72a81.png)

### Method 3: Write a nodejs application

Checkout the `NodeJsClient` example to know how to call our micro-service from a NodeJs / TypeScript application.

## Check if the previous step works!

Our *OrderService* is equiped a small web interface which display the database. This interface is accessible on `/index.html`.

![OrderService UI](https://user-images.githubusercontent.com/1638594/127823619-770b2cca-cda3-444b-9c33-b83a753cad03.png)

Each time the database get updated, our micro-service uses [SignalR](https://dotnet.microsoft.com/apps/aspnet/signalr) to notify changes to this interface and keep it always up to date (You don't have to refresh (F5) on the page)

# Play More...

You can also run 2 instances of the service

```
dotnet run --urls=http://localhost:5005
dotnet run --urls=http://localhost:5006
```

Each instance has its own (in-memory) database. When you call the *NewOrder* function (cf. Step 3 of the previous section), then one of the instance will create the *Order* in its database.

When you call *GetOrder* then it might return Error or not depend on the instance which executed your *GetOrder* request.

# This project structure is bad

For the sake of brevity The project structure + naming are.. bad. In practice you should split this `OrderService.API` into at least 3 different projects, the 2 others are:

* The Data Access Layer (`DAL` folder) should be on other project.
* The data contract should be on other project.

# Service contract and Service API documentation

When you want to use/consume a (micro-)service, you must to know about its contract (what is the input / output structure, what is the exchange name on RabbitMQ which we have to use?)

Our *OrderService* exposes a swagger interface which is used as a living documentation of the Micro-service. By looking at it you would be able to deduce How to call our service via Rabbitmq.

For example: Here is the contract of the *NewOrder* function

![NewOrder function contract](https://user-images.githubusercontent.com/1638594/130366091-2ead33bd-a803-4605-a091-d45be070fe6b.png)

the input structure is `OrderService.Contracts.NewOrderCommand` => we should publish the request to the exchange `OrderService.Contracts:NewOrderCommand` in RabbitMQ

We know the name and the structure of the message contract, there are `statusCode` and `statusText` so here the message we could publish in RabbitMQ

![Call service with rabbitmq UI](https://user-images.githubusercontent.com/1638594/130364739-28ca4682-8d9e-4cd4-abd7-2762a817e582.png)

I believe that maintaining a swagger documentation is far better than maintaining a separated documentation in a Readme file or a confluence / wiki. There are 2 advantages:

* The swagger documentation is alive. It evoles with your codes. If you refactor / rename or add more properties to the input output  structure then changes will automaticly appeared in the swagger interface. Or else, you will have to maintain your wiki or confluence document to make it synchronize with your actual code.

* If for support reason, you have to call a micro-service (for example GetOrder) sometimes you have to call it in production to investigate some problem / bug for example. You can easily do so with Postman / Swagger interface. Or else you will have to develop an application to call your service. There is no easy tool allowing you to call the service AND GET THE RESPONSE with RabbitMQ.

# Prometheus metric

Our *OrderService* exposes the metrics on `/metrics`
These metrics are consumable by Prometheus. Here how it works:

## Step 1) Run RabbitMQ and Prometheus on localhost

```
docker-compose -f docker-compose-infra.yml up
```
prometheus will be accessible on `localhost:9090`

## Step 2) Run the OrderService **with Kestrel** (not IIS Express). 

You can run/debug the *OrderService* From Visual Studio (don't forget to choose Kestrel) or from the command line

```
dotnet run
```

by default the service should be accessible on https://localhost:5001/swagger and the metrics should be accessible on https://localhost:5001/metrics

Our local prometheus server will read the metric from https://host.docker.internal:5001/metrics (see the `prometheus-config.yml`)

so you will have to [map the DNS `host.docker.internal` to `127.0.0.1`](https://stackoverflow.com/a/43541732) and **make sure that the following link works in your browser** (as in the screenshot)

![metrics](https://user-images.githubusercontent.com/1638594/127578693-ba142563-5442-45ef-97e1-6fdc606122ef.png)

## Step 3) Try it out

* Access to the prometheus interface via `http://localhost:9090/targets`
* Make sure that prometheus can "see" its target (our *OrderService*)

![prometheus targets](https://user-images.githubusercontent.com/1638594/127579581-93329846-7f85-42e4-a0eb-44c633bb5ddd.png)

* Make some request to *OrderService* as usual
* Navigate some metrics collected by Prometheus

![prometheus metric sample](https://user-images.githubusercontent.com/1638594/127786038-8b4d5fb3-bb44-43cb-b5cc-c09e4313ae52.png)

# Polymorphism

We'd like to make the exact same request and choose from 2 the differents queues so we will got different responses (of the same structure) depending on the queue we chosen.

For example, if we send the following request
```
{
  "messageType": [
    "urn:message:OrderService.Contracts.Psp:BuildPaymentForm"
  ],
  "message": {
    "reference": "abc",
    "amount": 1
  }
}
```
to the queue `OrderService.Contracts.Psp.AtosBuildPaymentFormHandler`, then we would obtain the following response
```
{
  "linkToPaymentPage": "https://atos.com/?ref=abc&amount=1",
  "method": "POST"
}
```
and if we send this same request to the queue `OrderService.Contracts.Psp.PayzenBuildPaymentFormHandler`, then we would obtain a different response (but same structure)
```
{
  "linkToPaymentPage": "https://payzen.net/?id=abc&amountraw=1",
  "method": "GET"
}
```

In order to acheive this effect

## On the server side

We registered 2 different consumers on 2 different queues. Both consumers take the same structure `BuildPaymentForm` as request and `BuildPaymentFormResponse` as response.

```C#
cfg.ReceiveEndpoint("OrderService.Contracts.Psp.AtosBuildPaymentFormHandler", rep => rep.ConfigureConsumer<Psp.AtosBuildPaymentFormHandler>(ctx));
cfg.ReceiveEndpoint("OrderService.Contracts.Psp.PayzenBuildPaymentFormHandler", rep => rep.ConfigureConsumer<Psp.PayzenBuildPaymentFormHandler>(ctx));
```

## On the client side

We can create a requester of `BuildPaymentForm` on a specific queue name (then get the response via the requester as usual).

[checkout full codes](master/src/OrderService.API/Controllers/Psp/BuildPaymentFormController%20.cs#L25-45)

```C#
string queueName = "OrderService.Contracts.Psp.AtosBuildPaymentFormHandler";
var requestClient = clientFactory.CreateRequestClient<BuildPaymentForm>(new Uri("queue:" + queueName));
var resu = await requestClient.GetResponse<BuildPaymentFormResponse>(input).ConfigureAwait(false);
```

# Circuit Breaker

In this project sample, the [CircuitBreaker](https://masstransit-project.com/advanced/middleware/circuit-breaker.html#circuit-breaker) is configured as following

```
"TopologyConfiguration": {
        "ErrorsTtl": 120000,
        "DeadLettersTtl": 120000,
        "CircuitBreaker": {
            "TrackingPeriod": 10000,
            "ActiveThreshold": 10,
            "TripThreshold": 10,
            "ResetInterval": 10000
        }
    },
```

It means that 
* winthin the `TrackingPeriod` of 10 seconds,
* we should get 10 messages (`ActiveThreshold`) before the circuit breaker start to evaluate the ratio.
* if the 10% failure is reached (`TripThreshold`) then the service is temporary closed for 10 seconds (`ResetInterval`)

In this example, We sent a order with a negative status every 0.5 sec. It cause a Fault on the server side ("Bad Order Status") for every 0.5 sec. The CircuitBreaker detected too much "faults" and trip (paused) for 10s each time. During this time, the client continue to send messages but get "TimeOut" as a response.

![image](https://user-images.githubusercontent.com/1638594/130370354-d5ee2ec1-924c-4ba3-a8e1-43a7af196e43.png)

# Advance request/reply pattern

Context:

https://stackoverflow.com/questions/68918940/masstransit-how-to-make-advanced-request-reply-pattern/68922832#68922832


![Advance Request Reply pattern](https://user-images.githubusercontent.com/1638594/130929265-74b50845-3896-4b86-8b29-acc1f78f06db.png)

In the scope of this "OrderService.API" We couldn't really showcase this pattern. Because we will need at least 2 micro-services (Or you can try to run 2 different instance of the "OrderService.API")


