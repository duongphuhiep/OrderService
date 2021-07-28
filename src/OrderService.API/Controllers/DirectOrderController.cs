using Microsoft.AspNetCore.Mvc;
using OrderService.API.DAL;
using OrderService.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderService.API.Controllers
{
    /// <summary>
    /// Access direct to database to do thing
    /// </summary>
    [ApiController]
    [Route("direct/order")]
    public class DirectOrderController : ControllerBase
    {
        private readonly IOrderRepository orderRepository;
        public DirectOrderController(IOrderRepository orderRepository)
        {
            this.orderRepository = orderRepository;
        }

        [HttpPut]
        public async Task<Order> AddNewOrder([FromBody] NewOrderCommand newOrder)
        {
            return await orderRepository.AddNewOrder(newOrder).ConfigureAwait(false);
        }

        [HttpGet("{orderId}")]
        public async Task<Order> GetOrder(string orderId)
        {
            return await orderRepository.GetOrder(orderId).ConfigureAwait(false);
        }

        [HttpGet("all")]
        public async Task<IEnumerable<Order>> GetAllOrder()
        {
            return await orderRepository.GetAll().ConfigureAwait(false);
        }
    }
}
