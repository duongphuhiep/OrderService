using MassTransit;
using Microsoft.AspNetCore.Mvc;
using OrderService.Contracts.Psp;
using OrderService.Psp;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;

namespace OrderService.API.Controllers.Psp
{
    /// <summary>
    /// Send request to Rabbitmq to do thing
    /// </summary>
    [ApiController]
    [Route("v1/[controller]")]
    [SwaggerTag("Sugar API endpoint: forward the request/response from/to RabbitMQ to execute the functions")]
    public class BuildPaymentFormController : ControllerBase
    {
        private readonly IClientFactory clientFactory;

        public BuildPaymentFormController(IClientFactory clientFactory)
        {
            this.clientFactory = clientFactory;
        }

        [HttpPost("/{pspName}")]
        [SwaggerOperation(Summary = "BuildPaymentForm")]
        public async Task<BuildPaymentFormResponse> AddNewOrder(string pspName, [FromBody] BuildPaymentForm input)
        {
            string queueName;
            if (pspName == "atos")
            {
                queueName = typeof(AtosBuildPaymentFormHandler).FullName;
            }
            else if (pspName == "payzen")
            {
                queueName = typeof(PayzenBuildPaymentFormHandler).FullName;
            }
            else
            {
                throw new ArgumentException("Unknow Psp");
            }
            var requestClient = clientFactory.CreateRequestClient<BuildPaymentForm>(new Uri("queue:" + queueName));
            var resu = await requestClient.GetResponse<BuildPaymentFormResponse>(input).ConfigureAwait(false);
            return resu.Message;
        }
    }
}
