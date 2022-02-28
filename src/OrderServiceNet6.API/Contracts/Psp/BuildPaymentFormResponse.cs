using System;

namespace OrderService.Contracts.Psp
{
    public record BuildPaymentFormResponse
    {
        public Uri LinkToPaymentPage { get; init; }
        public string Method { get; init; }
    }
}
