using MassTransit;
using OrderService.API.DAL;
using OrderService.Contracts;
using System.Threading.Tasks;

namespace OrderService.API.Consumers
{
    public class GetOrderQueryHandler : IConsumer<GetOrderQuery>
    {
        private readonly IOrderRepository orderRepository;

        public GetOrderQueryHandler(IOrderRepository orderRepository)
        {
            this.orderRepository = orderRepository;
        }

        public async Task Consume(ConsumeContext<GetOrderQuery> context)
        {
            var resu = await orderRepository.GetOrder(context.Message.OrderId).ConfigureAwait(false);
            await context.RespondAsync(resu).ConfigureAwait(false);
        }
    }
}
