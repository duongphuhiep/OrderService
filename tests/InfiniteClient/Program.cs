using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderService.API.Config;
using Serilog;
using Serilog.Events;
using System;

namespace InfiniteClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // The initial "bootstrap" logger is able to log errors during start-up. It's completely replaced by the
            // logger configured in `UseSerilog()` below, once configuration and dependency-injection have both been
            // set up successfully.
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "An unhandled exception occured during bootstrapping");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        static bool? _isRunningInContainer;
        static bool IsRunningInContainer =>
            _isRunningInContainer ??= bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out var inContainer) && inContainer;

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console())
                .ConfigureServices((hostContext, services) =>
                {
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
                                var rabbitMqConfiguration = hostContext.Configuration.GetSection("RabbitMqConfiguration").Get<RabbitMqConfiguration>();
                                cfg.Host(rabbitMqConfiguration?.Host ?? "localhost", rabbitMqConfiguration?.Port ?? 5672, rabbitMqConfiguration?.VirtualHost ?? "/", hostConfig =>
                                {
                                    hostConfig.Username(rabbitMqConfiguration?.UserName ?? "");
                                    hostConfig.Password(rabbitMqConfiguration?.Password ?? "");
                                });
                            }
                        });
                    })
                    //or declare a generic one for all requestClient
                    .AddMassTransitHostedService()
                    ;


                    #endregion config MassTransit/Rabitmq

                    services.AddHostedService<Worker>();
                });
    }
}
