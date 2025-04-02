using Microsoft.Extensions.Options;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shared.Configuration;

namespace Api.Extensions
{
    public static class OpenTelemetryExtensions
    {
        public static WebApplicationBuilder AddOpenTemlemetryConfiguration(this WebApplicationBuilder builder)
        {
            using var serviceProvider = builder.Services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<OpenTelemetrySettings>>();
            var exporter = options.Value.Exporter ?? "http://localhost:4317";
            builder.Services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService("vinnare"))
                .WithMetrics(metrics =>
                {
                    metrics
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddMeter("PurchasesMeter");


                    metrics.AddOtlpExporter(options => options.Endpoint = new Uri(exporter));
                })
                .WithTracing(tracing =>
                {
                    tracing
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddEntityFrameworkCoreInstrumentation();
                    tracing.AddOtlpExporter(options => options.Endpoint = new Uri(exporter));
                });

            builder.Logging.AddOpenTelemetry(logging => logging.AddOtlpExporter(options => options.Endpoint = new Uri(exporter)));
            return builder;
        }
    }
}
