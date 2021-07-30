using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.API.Models;
using System.Threading.Tasks;

namespace OrderService.API.Consumers
{
    public class OrderAddedEventHandler : IConsumer<OrderAddedEvent>
    {
        private readonly ILogger<OrderAddedEventHandler> log;

        public OrderAddedEventHandler(ILogger<OrderAddedEventHandler> log)
        {
            this.log = log;
        }
        public async Task Consume(ConsumeContext<OrderAddedEvent> context)
        {
            log.LogInformation("OrderAddedEvent Received: {@OrderAddedEvent}", context.Message);
        }
    }
}
