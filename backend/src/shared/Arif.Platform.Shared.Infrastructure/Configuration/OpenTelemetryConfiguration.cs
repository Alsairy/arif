using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;

namespace Arif.Platform.Shared.Infrastructure.Configuration;

public static class OpenTelemetryConfiguration
{
    public static IServiceCollection AddOpenTelemetryConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var serviceName = configuration["OpenTelemetry:ServiceName"] ?? "Arif.Platform";
        var serviceVersion = configuration["OpenTelemetry:ServiceVersion"] ?? "1.0.0";
        var jaegerEndpoint = configuration["OpenTelemetry:JaegerEndpoint"] ?? "http://localhost:14268/api/traces";
        var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://localhost:4317";

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName, serviceVersion)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = configuration["Environment"] ?? "development",
                    ["service.namespace"] = "arif.platform",
                    ["service.instance.id"] = Environment.MachineName
                }))
            .WithTracing(tracing => tracing
                .AddSource("Arif.Platform.*")
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.EnrichWithHttpRequest = (activity, request) =>
                    {
                        activity.SetTag("http.request.body.size", request.ContentLength);
                        activity.SetTag("http.request.header.user-agent", request.Headers.UserAgent.ToString());
                        activity.SetTag("http.request.header.x-forwarded-for", request.Headers["X-Forwarded-For"].FirstOrDefault());
                    };
                    options.EnrichWithHttpResponse = (activity, response) =>
                    {
                        activity.SetTag("http.response.body.size", response.ContentLength);
                        activity.SetTag("http.response.header.content-type", response.ContentType);
                    };
                })
                .AddHttpClientInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.EnrichWithHttpRequestMessage = (activity, request) =>
                    {
                        activity.SetTag("http.client.request.body.size", request.Content?.Headers?.ContentLength);
                    };
                    options.EnrichWithHttpResponseMessage = (activity, response) =>
                    {
                        activity.SetTag("http.client.response.body.size", response.Content?.Headers?.ContentLength);
                    };
                })
                .AddEntityFrameworkCoreInstrumentation(options =>
                {
                    options.SetDbStatementForText = true;
                    options.SetDbStatementForStoredProcedure = true;
                    options.EnrichWithIDbCommand = (activity, command) =>
                    {
                        activity.SetTag("db.command.timeout", command.CommandTimeout);
                        activity.SetTag("db.command.type", command.CommandType.ToString());
                    };
                })
                .AddRedisInstrumentation()
                .AddJaegerExporter(options =>
                {
                    options.Endpoint = new Uri(jaegerEndpoint);
                })
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                    options.Protocol = OtlpExportProtocol.Grpc;
                })
                .AddConsoleExporter())
            .WithMetrics(metrics => metrics
                .AddMeter("Arif.Platform.*")
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddPrometheusExporter()
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                    options.Protocol = OtlpExportProtocol.Grpc;
                })
                .AddConsoleExporter());

        return services;
    }

    public static IServiceCollection AddCustomMetrics(this IServiceCollection services)
    {
        services.AddSingleton<System.Diagnostics.Metrics.Meter>(provider =>
        {
            return new System.Diagnostics.Metrics.Meter("Arif.Platform.Custom", "1.0.0");
        });

        return services;
    }
}
