using MassTransit;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace OrderService.Psp
{
    public class AtosBuildPaymentFormHandler : IConsumer<BuildPaymentForm>
    {
        public async Task Consume(ConsumeContext<BuildPaymentForm> context)
        {
            await context.RespondAsync(new BuildPaymentFormResponse
            {
                LinkToPaymentPage = new Uri($"https://atos.com/?ref={context.Message.Reference}&amount={context.Message.Amount}"),
                Method = HttpMethod.Post
            }).ConfigureAwait(false);
        }
    }
}
