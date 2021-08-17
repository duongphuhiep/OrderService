using System;
using System.Net.Http;

namespace OrderService.Psp
{
    public record BuildPaymentFormResponse
    {
        public Uri LinkToPaymentPage { get; init; }
        public HttpMethod Method { get; init; }
    }
}
