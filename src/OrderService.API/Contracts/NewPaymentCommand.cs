namespace OrderService.Contracts
{
    public record NewPaymentCommand
    {
        public string PspName { get; init; }
        public string Reference { get; init; }
        public decimal Amount { get; init; }
    }
}
