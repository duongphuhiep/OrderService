using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.Formatters.Prometheus;
using MassTransit;
using OrderService.API.DAL;
using OrderService.API.SignalrHubs;
using Serilog;
using Serilog.Events;
using System.Reflection;

// The initial "bootstrap" logger is able to log errors during start-up. It's completely replaced by the
// logger configured in `UseSerilog()` below, once configuration and dependency-injection have both been
// set up successfully.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(Path.Combine(Path.GetTempPath(), "OrderService.log"))
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Host.UseSerilog();

#region Host Metrics

IMetricsRoot Metrics = AppMetrics.CreateDefaultBuilder()
    .OutputMetrics.AsPrometheusPlainText()
    .OutputMetrics.AsPrometheusProtobuf()
    .Build();
builder.Host.UseMetrics(
    options =>
    {
        options.EndpointOptions = endpointsOptions =>
        {
            endpointsOptions.MetricsTextEndpointOutputFormatter = Metrics.OutputMetricsFormatters.OfType<MetricsPrometheusTextOutputFormatter>().First();
            endpointsOptions.MetricsEndpointOutputFormatter = Metrics.OutputMetricsFormatters.OfType<MetricsPrometheusProtobufOutputFormatter>().First();
        };
    });

#endregion

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddSignalR();

#region Services MassTransit

builder.Services.AddMediator(cfg =>
{
    cfg.AddConsumers(Assembly.GetEntryAssembly());
})
.AddGenericRequestClient();

#endregion

#region Services Application

builder.Services.AddSingleton(typeof(IOrderRepository), typeof(OrderInMemoryRepository));

#endregion

#region Services Healthcheck

builder.Services.AddHealthChecks();

#endregion

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseStaticFiles("/ui");
app.UseAuthorization();
app.MapControllers();
app.MapHub<MainHub>("/mainhub");
#region App Healthcheck
app.MapHealthChecks("health");
#endregion

app.Run();
