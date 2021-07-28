using MassTransit;
using OrderService.API.DAL;
using OrderService.API.Models;
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
            var resu = await orderRepository.AddNewOrder(context.Message).ConfigureAwait(false);
            await context.RespondAsync(resu).ConfigureAwait(false);
        }
    }
}
