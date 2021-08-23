# Micro-service showcase

This is a experimental template micro-service project. It showcases the **communication between micro-service**.

We don't have any implementation of [Service Mesh](https://en.wikipedia.org/wiki/Service_mesh) in our infrastructure so we must to cover the mssing parts as much as possible, or at least the critical ones.

The main project is a fictif "OrderService.API" (which doesn't have much sense in term of Business Logic).

This same project shows both the "Consumer part" (or "the Server") and the "Producer part" (or "the Client")

* the "client" Produces requests (and possibily wait for a response)
* the "server" Consumes (or handle) the request and possibly return a response. In case of "crash" (server throw exception) the server would return a "Fault".

The communication between Client/Server (hence between micro-service) is made with help of the MassTransit framework, with RabbitMQ as transport

Most notably: 

Thanks to the [MassTransit framework](https://masstransit-project.com/), our microservice communication have the possibility to

- Swap the RabbitMQ transport to other supported transport such as Azur Service Bus, Amazon SQS or gRPC. But the big advantage is that we can swap to the In Memory transport in the **Unit test context**.
  
And appreciably, MassTransit/RabbitMQ covers some missing parts caused by our lack of a Service Mesh implementation:

- Retry + CircuitBreaker
- Metrics / Monitoring
- LoadBalancing

# Next step

Read the [README.md](src/OrderService.API/README.md) of the OrderService.API and get your hand dirty.

# References

* Watch [Istio & Service Mesh simply explained](https://www.youtube.com/watch?v=16fgzklcF7Y)
* Read [Dapr](https://docs.microsoft.com/en-us/dotnet/architecture/dapr-for-net-developers/dapr-at-20000-feet) 
