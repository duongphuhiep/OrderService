using MassTransit;
using MassTransit.Definition;
using MassTransit.PrometheusIntegration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OrderService.API.Config;
using OrderService.API.DAL;
using OrderService.API.SignalrHubs;
using Prometheus;
using Serilog;
using System;
using System.Reflection;

namespace OrderService.API
{
    public class Startup
    {
        static bool? _isRunningInContainer;
        static bool IsRunningInContainer =>
            _isRunningInContainer ??= bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out var inContainer) && inContainer;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "OrderService.API", Version = "v1" });
                c.CustomSchemaIds(t => t.FullName);
                c.EnableAnnotations();
            });

            services.AddSignalR();

            #region config MassTransit/Rabitmq

            _ = services.AddMassTransit(x =>
              {
                  x.UsingRabbitMq((ctx, cfg) =>
                  {
                      if (IsRunningInContainer)
                      {
                          cfg.Host("rabbitmq");
                      }
                      else
                      {
                          var rabbitMqConfiguration = Configuration.GetSection("RabbitMqConfiguration").Get<RabbitMqConfiguration>();
                          cfg.Host(rabbitMqConfiguration?.Host ?? "localhost", rabbitMqConfiguration?.Port ?? 5672, rabbitMqConfiguration?.VirtualHost ?? "/", hostConfig =>
                          {
                              hostConfig.Username(rabbitMqConfiguration?.UserName ?? "");
                              hostConfig.Password(rabbitMqConfiguration?.Password ?? "");
                          });
                      }
                      cfg.SendTopology.ConfigureErrorSettings = settings => settings.SetQueueArgument("x-message-ttl", 60000); //60000 * 60 * 24 * 2
                      cfg.ConfigureEndpoints(ctx, new KebabCaseEndpointNameFormatter(true));
                      cfg.UsePrometheusMetrics(serviceName: "order_service");

                      //polymorphisme: the same contract "BuildPaymentForm" is consumed on 2 different queues
                      cfg.ReceiveEndpoint(typeof(Psp.AtosBuildPaymentFormHandler).FullName, rep => rep.ConfigureConsumer<Psp.AtosBuildPaymentFormHandler>(ctx));
                      cfg.ReceiveEndpoint(typeof(Psp.PayzenBuildPaymentFormHandler).FullName, rep => rep.ConfigureConsumer<Psp.PayzenBuildPaymentFormHandler>(ctx));
                  });
                  //you can declare requestClient one by one on a customized queue/exchange
                  //x.AddRequestClient<Psp.BuildPaymentForm>(new Uri($"exchange:{typeof(Psp.PayzenBuildPaymentFormHandler).FullName}"));
                  //x.AddRequestClient<Psp.BuildPaymentForm>(new Uri($"exchange:{typeof(Psp.PayzenBuildPaymentFormHandler).FullName}"));
                  x.AddConsumers(Assembly.GetEntryAssembly()); //or x.AddConsumersFromNamespaceContaining<Startup>()
              })
            //or declare a generic one for all requestClient
            .AddGenericRequestClient()
            .AddMassTransitHostedService();

            #endregion config MassTransit/Rabitmq

            #region config DI (DAL / other services)

            services.AddSingleton(typeof(IOrderRepository), typeof(OrderInMemoryRepository));

            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderService.API v1"));
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMetrics();
                endpoints.MapControllers();
                endpoints.MapHub<MainHub>("/mainhub");
            });

            app.UseSerilogRequestLogging();
        }
    }
}
