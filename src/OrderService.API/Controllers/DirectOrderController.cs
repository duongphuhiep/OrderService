using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OrderService.API.DAL;
using OrderService.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderService.API.Controllers
{
    /// <summary>
    /// Access direct to database to do thing
    /// </summary>
    [ApiController]
    [Route("direct/order")]
    [SwaggerTag("These function will access to the Database directly. Use for test / demonstration only")]
    public class DirectOrderController : ControllerBase
    {
        private readonly IOrderRepository orderRepository;
        private readonly IConfiguration config;

        public DirectOrderController(IOrderRepository orderRepository, IConfiguration config)
        {
            this.orderRepository = orderRepository;
            this.config = config;
        }

        /// <summary>
        /// Create New Order
        /// </summary>
        [HttpPut]
        public async Task<Order> AddNewOrder([FromBody] NewOrderCommand newOrder)
        {
            return await orderRepository.AddNewOrder(newOrder).ConfigureAwait(false);
        }

        /// <summary>
        /// Get info of an Order
        /// </summary>
        [HttpGet("{orderId}")]
        public async Task<Order> GetOrder(string orderId)
        {
            return await orderRepository.GetOrder(orderId).ConfigureAwait(false);
        }

        /// <summary>
        /// Get All Order in the database
        /// </summary>
        [HttpGet("all")]
        public async Task<IEnumerable<Order>> GetAllOrder()
        {
            return await orderRepository.GetAll().ConfigureAwait(false);
        }

        [HttpGet("lemonurl")]
        public string GetLemonUrl()
        {
            return config.GetValue<string>("App:LemonUrl");
        }
    }
}
