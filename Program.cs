using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq;

internal class Program
{
    private static void Main(string[] args)
    {
        var host = new HostBuilder()
        .ConfigureFunctionsWebApplication()
        .ConfigureAppConfiguration(configBuilder =>
        {
            configBuilder.AddUserSecrets<Program>(optional: true, reloadOnChange: false);
        })
        .ConfigureServices(services =>
        {
            services.AddApplicationInsightsTelemetryWorkerService();
            services.ConfigureFunctionsApplicationInsights();
        })
        .ConfigureLogging(logging =>
        {
            // By default, the App Insights SDK adds a logging filter that instructs the logger to capture only warnings 
            // and more sever logs https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide?tabs=windows#managing-log-levels
            logging.Services.Configure<LoggerFilterOptions>(options =>
            {
                var defaultRule = options.Rules.FirstOrDefault(rule => rule.ProviderName
                    == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
        
                if (defaultRule is not null)
                {
                    options.Rules.Remove(defaultRule);
                }
        
                options.MinLevel = LogLevel.Information;
                options.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning);
                options.AddFilter("Microsoft.AspNetCore.Routing.EndpointMiddleware", LogLevel.Warning);
                options.AddFilter("Microsoft.AspNetCore.Mvc.Infrastructure.ObjectResultExecutor", LogLevel.Warning);
                options.AddFilter("Microsoft.DurableTask.Client.Grpc.GrpcDurableTaskClient", LogLevel.Warning);
                options.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning);
                options.AddFilter("Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager", LogLevel.Warning);
            });
        })
        .Build();

        host.Run();
    }
}