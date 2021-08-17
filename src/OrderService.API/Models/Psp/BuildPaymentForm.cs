namespace OrderService.Psp
{
    public record BuildPaymentForm
    {
        public string Reference { get; init; }
        public decimal Amount { get; init; }
    }
}
