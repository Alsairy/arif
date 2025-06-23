using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using Serilog.Formatting.Compact;

namespace Arif.Platform.Shared.Infrastructure.Configuration;

public static class LoggingConfiguration
{
    public static IServiceCollection AddStructuredLogging(this IServiceCollection services, IConfiguration configuration)
    {
        var elasticsearchUri = configuration["Logging:Elasticsearch:Uri"] ?? "http://localhost:9200";
        var indexFormat = configuration["Logging:Elasticsearch:IndexFormat"] ?? "arif-platform-logs-{0:yyyy.MM.dd}";
        var environment = configuration["Environment"] ?? "development";
        var serviceName = configuration["OpenTelemetry:ServiceName"] ?? "Arif.Platform";

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Environment", environment)
            .Enrich.WithProperty("ServiceName", serviceName)
            .Enrich.WithProperty("MachineName", Environment.MachineName)
            .Enrich.WithProperty("ProcessId", Environment.ProcessId)
            .Enrich.WithCorrelationId()
            .WriteTo.Console(new CompactJsonFormatter())
            .WriteTo.File(
                new CompactJsonFormatter(),
                path: "logs/arif-platform-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                fileSizeLimitBytes: 100 * 1024 * 1024,
                rollOnFileSizeLimit: true)
            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticsearchUri))
            {
                IndexFormat = indexFormat,
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                TemplateName = "arif-platform-template",
                TypeName = "_doc",
                BatchAction = ElasticOpType.Index,
                Period = TimeSpan.FromSeconds(2),
                InlineFields = true,
                MinimumLogEventLevel = LogEventLevel.Information,
                EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                                 EmitEventFailureHandling.WriteToFailureSink |
                                 EmitEventFailureHandling.RaiseCallback,
                FailureSink = new Serilog.Sinks.File.FileSink("logs/elasticsearch-failures-.log", new CompactJsonFormatter(), null),
                DeadLetterIndexName = "arif-platform-deadletter-{0:yyyy.MM.dd}",
                RegisterTemplateFailure = RegisterTemplateRecovery.IndexAnyway,
                OverwriteTemplate = false,
                NumberOfShards = 2,
                NumberOfReplicas = 1,
                CustomFormatter = new CompactJsonFormatter(),
                ModifyConnectionSettings = connectionConfiguration =>
                    connectionConfiguration.BasicAuthentication(
                        configuration["Logging:Elasticsearch:Username"],
                        configuration["Logging:Elasticsearch:Password"])
            })
            .CreateLogger();

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(Log.Logger, dispose: true);
        });

        return services;
    }

    public static IServiceCollection AddRequestLogging(this IServiceCollection services)
    {
        services.AddSingleton<ILoggerFactory, LoggerFactory>();
        
        return services;
    }
}

public static class SerilogExtensions
{
    public static LoggerConfiguration WithCorrelationId(this LoggerEnrichmentConfiguration enrichmentConfiguration)
    {
        return enrichmentConfiguration.With<CorrelationIdEnricher>();
    }
}

public class CorrelationIdEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var correlationId = System.Diagnostics.Activity.Current?.Id ?? Guid.NewGuid().ToString();
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CorrelationId", correlationId));
        
        var traceId = System.Diagnostics.Activity.Current?.TraceId.ToString();
        if (!string.IsNullOrEmpty(traceId))
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TraceId", traceId));
        }
        
        var spanId = System.Diagnostics.Activity.Current?.SpanId.ToString();
        if (!string.IsNullOrEmpty(spanId))
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("SpanId", spanId));
        }
    }
}
