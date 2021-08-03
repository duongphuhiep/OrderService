namespace OrderService.API.Models
{
    public record GetOrderQuery
    {
        public string OrderId { get; init; }
    }
}
