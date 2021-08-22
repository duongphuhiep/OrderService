using Microsoft.Extensions.Logging;
using OrderService.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.API.DAL
{
    public interface IOrderRepository
    {
        Task<Order> AddNewOrder(NewOrderCommand newOrder);
        Task<Order> GetOrder(string id);
        Task<IEnumerable<Order>> GetAll();
    }

    public class OrderInMemoryRepository : IOrderRepository
    {
        private readonly ILogger<OrderInMemoryRepository> log;
        private readonly Dictionary<string, Order> repo = new Dictionary<string, Order>();

        public OrderInMemoryRepository(ILogger<OrderInMemoryRepository> log)
        {
            this.log = log;
        }
        public Task<Order> AddNewOrder(NewOrderCommand newOrder)
        {
            if (newOrder.StatusCode < 0)
            {
                throw new ArgumentException("Bad status code");
            }
            string newId = Guid.NewGuid().ToString();
            Order newOrderGenerated = new()
            {
                Id = newId,
                DateCreated = DateTime.UtcNow,
                StatusCode = newOrder.StatusCode,
                StatusText = newOrder.StatusText
            };
            repo[newId] = newOrderGenerated;
            log.LogInformation($"New order added: {newId}");
            return Task.FromResult(newOrderGenerated);
        }

        public Task<Order> GetOrder(string id)
        {
            log.LogInformation($"Get order: {id}");
            Order found;
            repo.TryGetValue(id, out found);
            return Task.FromResult(found);
        }

        public Task<IEnumerable<Order>> GetAll()
        {
            log.LogInformation($"Get All");
            return Task.FromResult(repo.Values.AsEnumerable());
        }
    }
}
