using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Api.Extensions
{
    public static class OpenTelemetryExtensions
    {
        public static WebApplicationBuilder AddOpenTemlemetryConfiguration(this WebApplicationBuilder builder)
        {
            builder.Services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService("vinnare"))
                .WithMetrics(metrics =>
                {
                    metrics
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddMeter("PurchasesMeter");


                    metrics.AddOtlpExporter(options => options.Endpoint = new Uri("http://localhost:4317"));
                })
                .WithTracing(tracing =>
                {
                    tracing
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddEntityFrameworkCoreInstrumentation();
                    tracing.AddOtlpExporter(options => options.Endpoint = new Uri("http://localhost:4317"));
                });

            builder.Logging.AddOpenTelemetry(logging => logging.AddOtlpExporter(options => options.Endpoint = new Uri("http://localhost:4317")));
            return builder;
        }
    }
}
