namespace OrderService.Contracts
{
    public record NewPaymentCommand
    {
        public string Reference { get; init; }
        public decimal Amount { get; init; }
    }
}
