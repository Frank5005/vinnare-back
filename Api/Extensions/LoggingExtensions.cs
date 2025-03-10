using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Api.Extensions
{
    public static class LoggingExtensions
    {
        public static IServiceCollection AddLoggingConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddConsole(); // Basic Console Logging

                // Placeholder for Cloud Logging (Example: Serilog, AWS CloudWatch, etc.)
                // loggingBuilder.AddSerilog();
            });

            return services;
        }
    }
}
