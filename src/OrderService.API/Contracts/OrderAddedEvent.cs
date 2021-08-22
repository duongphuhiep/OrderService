namespace OrderService.Contracts
{
    public record OrderAddedEvent
    {
        public string OrderId { get; init; }
        public string AddedBy { get; init; }
    }
}
