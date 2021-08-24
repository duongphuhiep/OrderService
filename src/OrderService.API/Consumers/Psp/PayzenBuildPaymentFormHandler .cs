using MassTransit;
using OrderService.Contracts.Psp;
using System;
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
                Method = "GET"
            }).ConfigureAwait(false);
        }
    }
}
