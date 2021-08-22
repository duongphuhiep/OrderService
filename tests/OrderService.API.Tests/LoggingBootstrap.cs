using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace NUnitTestBase
{
    /// <summary>
    /// we can also create a global Setup / Tears Down for all tests:
    /// https://github.com/nunit/docs/wiki/SetUpFixture-Attribute
    /// we don't need to use this functionality for now..
    /// </summary>
    public static class LoggingBootstrap
    {
        public static IServiceCollection AddSerilogNunit(this IServiceCollection services)
        {
            return services.AddLogging(builder =>
            {
                builder
                    .AddSerilog()
                    .SetMinimumLevel(LogLevel.Debug)
                    .AddFilter((category, logLevel) =>
                    {
                        if (category.StartsWith("Microsoft."))
                            return logLevel > LogLevel.Warning;
                        else if (category.StartsWith("System."))
                            return logLevel > LogLevel.Warning;
                        else return true;
                    });
            });
        }

        static LoggingBootstrap()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.NUnitOutput()
                .CreateLogger();
        }
    }
}