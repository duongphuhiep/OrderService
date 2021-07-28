This project is a fictif *OrderService* which exposes 2 functions:

* *NewOrder*: Create a new order (and save it to a volatile in-memory database)
* *GetOrder*: Get order information from its *id*

# How to play

## Step 1) Start rabbitmq on localhost

```
docker-compose -f docker-compose-rabbitmq.yml up
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

## Step 3) Call (or consume) the service

The following instruction will help you call the *NewOrder* function from our *OrderService*

### Method 1: Use the RabbitMQ UI

![Call service with rabbitmq UI](https://user-images.githubusercontent.com/1638594/127403732-04530c6d-f05c-4d4d-961b-e01061780106.png)

When the `OrderService.API` application is started, it would create the exchange `OrderService.API.Models:NewOrderCommand`

You can publish the following message to this exchange in order to execute the *NewOrder* function
```
{
  "messageType": [
    "urn:message:OrderService.API.Models:NewOrderCommand"
  ],
  "message": {
    "statusCode": 4,
    "statusText": "4"
  }
}
```

Unfortunately this way we cannot see the response of the *NewOrder* function (at least I don't know how)

### Method 2: Use Postman (or Swagger)

![Call service with Postman](https://user-images.githubusercontent.com/1638594/127404837-7d4a8d35-eb7d-44ba-b322-bfea38ae8ce5.png)

Behind the scene, when `OrderController` received this (postman) request, it publish on RabbitMQ (as you did in the Method (1) to execute the *NewOrder*. It will also return the reply from RabbitMQ.

## Step.. Bonus: Check if the previous step works!

In previous step, you called the *NewOrder* function to create a new order. Now:

1) This request will list all the Order you created by reading directly from the database

![Get all order](https://user-images.githubusercontent.com/1638594/127406797-c67d11e8-b5d5-4340-b562-48c7a1e32849.png)

2) This request will get information about 1 order. It uses rabbitmq to execute the *GetOrder* function and return the result. You could also execute this function from the RabbitMQ UI

![Get 1 order](https://user-images.githubusercontent.com/1638594/127448954-0621541c-a7c2-45d5-8941-d8d3bc185f16.png)

# Play More...

You can also run 2 instances of the service

```
dotnet run --urls=http://localhost:5005
dotnet run --urls=http://localhost:5006
```

Each instance has its own (in-memory) database. When you call the *NewOrder* function (cf. Step 3 of the previous section), then one of the instance will create the *Order* in its database.

When you call *GetOrder* then it might return Error or not depend on the instance which executed your *GetOrder* request.

# Project structure

The project structure + naming is.. bad. In practice you should split it to 3 different projects

* The Data Access Layer (`DAL` folder) should be on other project.
* The data contract (`Models` folder) should be on other project.
  * The data contract should be immutable interface (interface with getter only

# Service contract

When you want to use/consume a (micro-)service, you must to know about its contract (what is the input / output structure, what is the exchange name on RabbitMQ which we have to use?)

Our *OrderService* exposes a swagger interface. By looking at it you would be able to deduce How to call our service via rabbitmq.

For example: Here is the contract of the *NewOrder* function

![NewOrder service contract](https://user-images.githubusercontent.com/1638594/127469913-27cb5259-51d8-4952-a968-abbbfb3f8c86.png)

the input structure is `OrderService.API.Models.NewOrderCommand` => we should publish the request to the exchange `OrderService.API.Models:NewOrderCommand` in RabbitMQ

We know the name and the structure of the message contract, there are `statusCode` and `statusText` so here the message we could publish in RabbitMQ

![Call service with rabbitmq UI](https://user-images.githubusercontent.com/1638594/127403732-04530c6d-f05c-4d4d-961b-e01061780106.png)






