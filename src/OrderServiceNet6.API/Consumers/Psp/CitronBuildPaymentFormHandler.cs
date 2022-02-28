using MassTransit;
using OrderService.API.Config;
using OrderService.Contracts.Psp;

namespace OrderService.Psp
{
    public class CitronBuildPaymentFormHandler : IConsumer<BuildPaymentForm>
    {
        public async Task Consume(ConsumeContext<BuildPaymentForm> context)
        {
            await context.RespondAsync(new BuildPaymentFormResponse
            {
                LinkToPaymentPage = new Uri($"https://citron.org/?app={StaticAppId.Value}id={context.Message.Reference}&amountraw={context.Message.Amount}"),
                Method = "POST"
            }).ConfigureAwait(false);
        }
    }
}
