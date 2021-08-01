using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using OrderService.API.Models;
using OrderService.API.SignalrHubs;
using System.Threading.Tasks;

namespace OrderService.API.Consumers
{
    public class OrderAddedEventHandler : IConsumer<OrderAddedEvent>
    {
        private readonly ILogger<OrderAddedEventHandler> log;
        private readonly IHubContext<MainHub> mainHubContext;

        public OrderAddedEventHandler(ILogger<OrderAddedEventHandler> log, IHubContext<MainHub> mainHubContext)
        {
            this.log = log;
            this.mainHubContext = mainHubContext;
        }
        public async Task Consume(ConsumeContext<OrderAddedEvent> context)
        {
            log.LogInformation("OrderAddedEvent Received: {@OrderAddedEvent}", context.Message);
            await mainHubContext.Clients.All.SendAsync("DatabaseChanged", context.Message).ConfigureAwait(false);
        }
    }
}
