using System;

namespace OrderService.API.Models
{
    public record Order
    {
        public string Id { get; init; }
        public DateTime DateCreated { get; init; }
        public int StatusCode { get; init; }
        public string StatusText { get; init; }
    }
}
