namespace OrderService.Contracts
{
    public record NewOrderCommand
    {
        public int StatusCode { get; init; }
        public string StatusText { get; init; }
    }
}
