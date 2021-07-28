using System;

namespace OrderService.API.Models
{
    public class Order
    {
        public string Id { get; set; }
        public DateTime DateCreated { get; set; }
        public int StatusCode { get; set; }
        public string StatusText { get; set; }
    }
}
