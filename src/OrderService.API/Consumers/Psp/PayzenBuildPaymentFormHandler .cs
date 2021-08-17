using MassTransit;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace OrderService.Psp
{
    public class PayzenBuildPaymentFormHandler : IConsumer<BuildPaymentForm>
    {
        public async Task Consume(ConsumeContext<BuildPaymentForm> context)
        {
            await context.RespondAsync(new BuildPaymentFormResponse
            {
                LinkToPaymentPage = new Uri($"https://payzen.net/?id={context.Message.Reference}&amountraw={context.Message.Amount}"),
                Method = HttpMethod.Get
            }).ConfigureAwait(false);
        }
    }
}
