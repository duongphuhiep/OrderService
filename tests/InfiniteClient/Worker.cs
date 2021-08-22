using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderService.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace InfiniteClient
{
    public class Worker : BackgroundService
    {
        private readonly IClientFactory clientFactory;
        private readonly ILogger<Worker> _logger;

        public Worker(IClientFactory clientFactory, ILogger<Worker> logger)
        {
            _logger = logger;
            this.clientFactory = clientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var requestClient = clientFactory.CreateRequestClient<NewOrderCommand>(TimeSpan.FromSeconds(3));
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var _ = await requestClient.GetResponse<Order>(new NewOrderCommand
                    {
                        StatusCode = -1
                    }).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation("Send and get error: " + ex.Message);
                }
                await Task.Delay(500, stoppingToken).ConfigureAwait(false);
            }
        }
    }
}
