using MassTransit;
using OrderService.API.Config;
using OrderService.API.DAL;
using OrderService.Contracts;
using System;
using System.Threading.Tasks;

namespace OrderService.API.Consumers
{
    public class NewOrderCommandHandler : IConsumer<NewOrderCommand>
    {
        private readonly IOrderRepository orderRepository;

        public NewOrderCommandHandler(IOrderRepository orderRepository)
        {
            this.orderRepository = orderRepository;
        }

        public async Task Consume(ConsumeContext<NewOrderCommand> context)
        {
            if (context.Message.StatusCode < 0)
            {
                throw new ArgumentException("Bad status code");
            }
            var newOrder = await orderRepository.AddNewOrder(context.Message).ConfigureAwait(false);
            await context.RespondAsync(newOrder).ConfigureAwait(false);
            await context.Publish<OrderAddedEvent>(new()
            {
                AddedBy = StaticAppId.Value,
                OrderId = newOrder.Id
            }).ConfigureAwait(false);
        }
    }
}
