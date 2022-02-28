using MassTransit;
using OrderService.Contracts;
using OrderService.Contracts.Psp;
using System;
using System.Threading.Tasks;

namespace OrderService.API.Consumers
{
    public class NewPaymentCommandHandler : IConsumer<NewPaymentCommand>
    {
        public async Task Consume(ConsumeContext<NewPaymentCommand> context)
        {
            var pipe = context.CreateCopyContextPipe();
            await context.Send(new Uri("queue:" + typeof(Psp.CitronBuildPaymentFormHandler).FullName), new BuildPaymentForm
            {
                Amount = context.Message.Amount,
                Reference = context.Message.Reference
            }, pipe).ConfigureAwait(false);
        }
    }
}
