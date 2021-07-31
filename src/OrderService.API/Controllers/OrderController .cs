using MassTransit;
using Microsoft.AspNetCore.Mvc;
using OrderService.API.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;

namespace OrderService.API.Controllers
{
    /// <summary>
    /// Send request to Rabbitmq to do thing
    /// </summary>
    [ApiController]
    [Route("v1/[controller]")]
    [SwaggerTag("Sugar API endpoint: forward the request/response from/to RabbitMQ to execute the functions")]
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
        [SwaggerOperation(Summary = "NewOrder function", Description = "Create a new Order")]
        public async Task<Order> AddNewOrder([FromBody] NewOrderCommand newOrderCommand)
        {
            var resu = await requestClientNewOrder.GetResponse<Order>(newOrderCommand).ConfigureAwait(false);
            return resu.Message;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "GetOrder function", Description = "Get Order information")]
        public async Task<Order> GetOrder([FromBody] GetOrderQuery getOrderQuery)
        {
            var resu = await requestClientGetOrder.GetResponse<Order>(getOrderQuery).ConfigureAwait(false);
            return resu.Message;
        }
    }
}
