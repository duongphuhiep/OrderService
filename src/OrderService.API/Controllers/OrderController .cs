using MassTransit;
using Microsoft.AspNetCore.Mvc;
using OrderService.API.Models;
using System.Threading.Tasks;

namespace OrderService.API.Controllers
{
    /// <summary>
    /// Send request to Rabbitmq to do thing
    /// </summary>
    [ApiController]
    [Route("v1/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IRequestClient<NewOrderCommand> requestClientNewOrder;
        private readonly IRequestClient<GetOrderQuery> requestClientGetOrder;

        public OrderController(IRequestClient<NewOrderCommand> requestClientNewOrder, IRequestClient<GetOrderQuery> requestClientGetOrder)
        {
            this.requestClientNewOrder = requestClientNewOrder;
            this.requestClientGetOrder = requestClientGetOrder;
        }

        [HttpPut]
        public async Task<Order> AddNewOrder([FromBody] NewOrderCommand newOrderCommand)
        {
            var resu = await requestClientNewOrder.GetResponse<Order>(newOrderCommand).ConfigureAwait(false);
            return resu.Message;
        }

        [HttpGet]
        public async Task<Order> GetOrder([FromBody] GetOrderQuery getOrderQuery)
        {
            var resu = await requestClientGetOrder.GetResponse<Order>(getOrderQuery).ConfigureAwait(false);
            return resu.Message;
        }
    }
}
