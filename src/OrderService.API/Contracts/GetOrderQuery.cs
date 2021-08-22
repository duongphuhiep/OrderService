namespace OrderService.Contracts
{
    public record GetOrderQuery
    {
        public string OrderId { get; init; }
    }
}
