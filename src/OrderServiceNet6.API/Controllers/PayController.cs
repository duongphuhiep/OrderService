using MassTransit;
using Microsoft.AspNetCore.Mvc;
using OrderService.Contracts;
using OrderService.Contracts.Psp;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;

namespace OrderService.API.Controllers
{
    /// <summary>
    /// Send request to Rabbitmq to do thing
    /// </summary>
    [ApiController]
    [Route("v1/[controller]")]
    [SwaggerTag("Sugar API endpoint: forward the request/response from/to RabbitMQ to execute the functions")]
    public class PayController : ControllerBase
    {
        private readonly IRequestClient<NewPaymentCommand> requester;

        public PayController(IRequestClient<NewPaymentCommand> requester)
        {
            this.requester = requester;
        }

        [HttpPost]
        [SwaggerOperation(Summary = "NewPayment function", Description = "Create a new payment")]
        public async Task<BuildPaymentFormResponse> NewPayment([FromBody] NewPaymentCommand newPaymentCommand)
        {
            var resu = await requester.GetResponse<BuildPaymentFormResponse>(newPaymentCommand).ConfigureAwait(false);
            return resu.Message;
        }
    }
}
