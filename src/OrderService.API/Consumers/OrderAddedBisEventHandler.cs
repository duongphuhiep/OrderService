using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.API.Models;
using System.Threading.Tasks;

namespace OrderService.API.Consumers
{
    public class OrderAddedBisEventHandler : IConsumer<OrderAddedEvent>
    {
        private readonly ILogger<OrderAddedEventHandler> log;

        public OrderAddedBisEventHandler(ILogger<OrderAddedEventHandler> log)
        {
            this.log = log;
        }
        public async Task Consume(ConsumeContext<OrderAddedEvent> context)
        {
            log.LogInformation("OrderAddedEvent Bis Received: {@OrderAddedEvent}", context.Message);
        }
    }
}
