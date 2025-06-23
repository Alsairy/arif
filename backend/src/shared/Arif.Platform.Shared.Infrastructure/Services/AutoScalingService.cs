using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Arif.Platform.Shared.Common.Security;

namespace Arif.Platform.Shared.Infrastructure.Services;

public interface IAutoScalingService
{
    Task<ScalingDecision> EvaluateScalingNeedsAsync(string serviceName, CancellationToken cancellationToken = default);
    Task<bool> ScaleServiceAsync(string serviceName, ScalingAction action, CancellationToken cancellationToken = default);
    Task<List<ServiceMetrics>> GetServiceMetricsAsync(CancellationToken cancellationToken = default);
    Task<ScalingPolicy> GetScalingPolicyAsync(string serviceName, CancellationToken cancellationToken = default);
    Task<bool> UpdateScalingPolicyAsync(string serviceName, ScalingPolicy policy, CancellationToken cancellationToken = default);
    
    Task<PredictiveScalingRecommendation> GetPredictiveScalingRecommendationAsync(string serviceName, TimeSpan forecastWindow, CancellationToken cancellationToken = default);
    Task<CostOptimizationReport> GenerateCostOptimizationReportAsync(string serviceName, CancellationToken cancellationToken = default);
    Task<MultiCloudResourceAllocation> OptimizeMultiCloudAllocationAsync(List<string> serviceNames, CancellationToken cancellationToken = default);
    Task<bool> EnablePredictiveScalingAsync(string serviceName, PredictiveScalingConfig config, CancellationToken cancellationToken = default);
    Task<List<CloudProviderMetrics>> GetMultiCloudMetricsAsync(CancellationToken cancellationToken = default);
    Task<ResourceOptimizationSuggestions> GetResourceOptimizationSuggestionsAsync(string serviceName, CancellationToken cancellationToken = default);
}

public class AutoScalingService : IAutoScalingService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AutoScalingService> _logger;
    private readonly IAuditLogger _auditLogger;
    private readonly Dictionary<string, List<ServiceMetrics>> _historicalMetrics;
    private readonly Dictionary<string, PredictiveScalingConfig> _predictiveConfigs;
    private readonly Timer _metricsCollectionTimer;

    public AutoScalingService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AutoScalingService> logger,
        IAuditLogger auditLogger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _auditLogger = auditLogger;
        _historicalMetrics = new Dictionary<string, List<ServiceMetrics>>();
        _predictiveConfigs = new Dictionary<string, PredictiveScalingConfig>();
        
        _metricsCollectionTimer = new Timer(CollectHistoricalMetrics, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
    }

    public async Task<ScalingDecision> EvaluateScalingNeedsAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Evaluating scaling needs for service {ServiceName}", serviceName);

            var metrics = await GetCurrentMetricsAsync(serviceName, cancellationToken);
            var policy = await GetScalingPolicyAsync(serviceName, cancellationToken);

            var decision = new ScalingDecision
            {
                ServiceName = serviceName,
                CurrentInstances = metrics.InstanceCount,
                EvaluatedAt = DateTime.UtcNow
            };

            if (_predictiveConfigs.ContainsKey(serviceName))
            {
                var predictiveRecommendation = await GetPredictiveScalingRecommendationAsync(serviceName, TimeSpan.FromMinutes(30), cancellationToken);
                decision.PredictiveRecommendation = predictiveRecommendation;
                
                if (predictiveRecommendation.RecommendedAction != ScalingAction.NoAction)
                {
                    decision.RecommendedAction = predictiveRecommendation.RecommendedAction;
                    decision.TargetInstances = predictiveRecommendation.RecommendedInstances;
                    decision.Reason = $"Predictive scaling: {predictiveRecommendation.Reason}";
                    decision.ConfidenceScore = predictiveRecommendation.ConfidenceScore;
                    
                    await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, "Predictive scaling decision made", null, null, new { ServiceName = serviceName, Action = decision.RecommendedAction });
                    return decision;
                }
            }

            var costOptimization = await GenerateCostOptimizationReportAsync(serviceName, cancellationToken);
            decision.CostOptimization = costOptimization;

            if (metrics.CpuUtilization > policy.ScaleUpCpuThreshold || 
                metrics.MemoryUtilization > policy.ScaleUpMemoryThreshold ||
                metrics.RequestsPerSecond > policy.ScaleUpRpsThreshold)
            {
                decision.RecommendedAction = ScalingAction.ScaleUp;
                var targetInstances = Math.Min(metrics.InstanceCount + policy.ScaleUpIncrement, policy.MaxInstances);
                
                if (costOptimization.RecommendedInstanceType != metrics.InstanceType)
                {
                    decision.RecommendedInstanceType = costOptimization.RecommendedInstanceType;
                    decision.EstimatedCostSavings = costOptimization.PotentialSavings;
                }
                
                decision.TargetInstances = targetInstances;
                decision.Reason = "High resource utilization detected";
            }
            else if (metrics.CpuUtilization < policy.ScaleDownCpuThreshold && 
                     metrics.MemoryUtilization < policy.ScaleDownMemoryThreshold &&
                     metrics.RequestsPerSecond < policy.ScaleDownRpsThreshold &&
                     metrics.InstanceCount > policy.MinInstances)
            {
                decision.RecommendedAction = ScalingAction.ScaleDown;
                decision.TargetInstances = Math.Max(metrics.InstanceCount - policy.ScaleDownDecrement, policy.MinInstances);
                decision.Reason = "Low resource utilization detected";
                decision.EstimatedCostSavings = costOptimization.PotentialSavings;
            }
            else
            {
                decision.RecommendedAction = ScalingAction.NoAction;
                decision.TargetInstances = metrics.InstanceCount;
                decision.Reason = "Resource utilization within acceptable range";
            }

            return decision;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating scaling needs for service {ServiceName}", serviceName);
            throw;
        }
    }

    public async Task<bool> ScaleServiceAsync(string serviceName, ScalingAction action, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Executing scaling action {Action} for service {ServiceName}", action, serviceName);

            var cloudProviders = await GetActiveCloudProvidersAsync(serviceName, cancellationToken);
            var scalingResults = new List<bool>();

            foreach (var provider in cloudProviders)
            {
                var result = await ScaleOnCloudProviderAsync(serviceName, action, provider, cancellationToken);
                scalingResults.Add(result);
            }

            var overallSuccess = scalingResults.All(r => r);
            
            if (overallSuccess)
            {
                _logger.LogInformation("Successfully executed scaling action {Action} for service {ServiceName} across all cloud providers", action, serviceName);
                await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, "Scaling action executed", null, null, new { ServiceName = serviceName, Action = action, CloudProviders = cloudProviders.Select(p => p.Name) });
            }
            else
            {
                _logger.LogWarning("Partial failure executing scaling action {Action} for service {ServiceName}", action, serviceName);
            }

            return overallSuccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing scaling action for service {ServiceName}", serviceName);
            return false;
        }
    }

    public async Task<List<ServiceMetrics>> GetServiceMetricsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var metricsEndpoint = _configuration["Metrics:Endpoint"] ?? "http://localhost:9090";
            var response = await _httpClient.GetAsync($"{metricsEndpoint}/api/v1/metrics/services", cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var metrics = JsonSerializer.Deserialize<List<ServiceMetrics>>(responseContent) ?? new List<ServiceMetrics>();

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving service metrics");
            throw;
        }
    }

    public async Task<ScalingPolicy> GetScalingPolicyAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        try
        {
            var configEndpoint = _configuration["Configuration:Endpoint"] ?? "http://localhost:8087";
            var response = await _httpClient.GetAsync($"{configEndpoint}/api/v1/scaling-policies/{serviceName}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var policy = JsonSerializer.Deserialize<ScalingPolicy>(responseContent);
                return policy ?? GetDefaultScalingPolicy(serviceName);
            }
            else
            {
                return GetDefaultScalingPolicy(serviceName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error retrieving scaling policy for service {ServiceName}, using default", serviceName);
            return GetDefaultScalingPolicy(serviceName);
        }
    }

    public async Task<bool> UpdateScalingPolicyAsync(string serviceName, ScalingPolicy policy, CancellationToken cancellationToken = default)
    {
        try
        {
            var configEndpoint = _configuration["Configuration:Endpoint"] ?? "http://localhost:8087";
            var json = JsonSerializer.Serialize(policy);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{configEndpoint}/api/v1/scaling-policies/{serviceName}", content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully updated scaling policy for service {ServiceName}", serviceName);
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to update scaling policy for service {ServiceName}. Status: {StatusCode}", 
                    serviceName, response.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating scaling policy for service {ServiceName}", serviceName);
            return false;
        }
    }

    private async Task<ServiceMetrics> GetCurrentMetricsAsync(string serviceName, CancellationToken cancellationToken)
    {
        var allMetrics = await GetServiceMetricsAsync(cancellationToken);
        return allMetrics.FirstOrDefault(m => m.ServiceName == serviceName) ?? new ServiceMetrics
        {
            ServiceName = serviceName,
            InstanceCount = 1,
            CpuUtilization = 0,
            MemoryUtilization = 0,
            RequestsPerSecond = 0
        };
    }

    private static ScalingPolicy GetDefaultScalingPolicy(string serviceName)
    {
        return new ScalingPolicy
        {
            ServiceName = serviceName,
            MinInstances = 1,
            MaxInstances = 10,
            ScaleUpCpuThreshold = 70.0,
            ScaleDownCpuThreshold = 30.0,
            ScaleUpMemoryThreshold = 80.0,
            ScaleDownMemoryThreshold = 40.0,
            ScaleUpRpsThreshold = 1000,
            ScaleDownRpsThreshold = 100,
            ScaleUpIncrement = 2,
            ScaleDownDecrement = 1,
            CooldownPeriodMinutes = 5
        };
    }

    #region Enterprise Features Implementation

    public async Task<PredictiveScalingRecommendation> GetPredictiveScalingRecommendationAsync(string serviceName, TimeSpan forecastWindow, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating predictive scaling recommendation for service {ServiceName}", serviceName);

            var historicalData = GetHistoricalMetricsForService(serviceName);
            if (historicalData.Count < 10)
            {
                return new PredictiveScalingRecommendation
                {
                    ServiceName = serviceName,
                    RecommendedAction = ScalingAction.NoAction,
                    RecommendedInstances = 0,
                    Reason = "Insufficient historical data for prediction",
                    ConfidenceScore = 0.0,
                    ForecastWindow = forecastWindow
                };
            }

            var prediction = await PredictResourceNeedsAsync(historicalData, forecastWindow, cancellationToken);
            var currentMetrics = await GetCurrentMetricsAsync(serviceName, cancellationToken);
            var policy = await GetScalingPolicyAsync(serviceName, cancellationToken);

            var recommendation = new PredictiveScalingRecommendation
            {
                ServiceName = serviceName,
                ForecastWindow = forecastWindow,
                PredictedCpuUtilization = prediction.CpuUtilization,
                PredictedMemoryUtilization = prediction.MemoryUtilization,
                PredictedRequestsPerSecond = prediction.RequestsPerSecond,
                ConfidenceScore = prediction.ConfidenceScore
            };

            if (prediction.CpuUtilization > policy.ScaleUpCpuThreshold || 
                prediction.MemoryUtilization > policy.ScaleUpMemoryThreshold ||
                prediction.RequestsPerSecond > policy.ScaleUpRpsThreshold)
            {
                recommendation.RecommendedAction = ScalingAction.ScaleUp;
                recommendation.RecommendedInstances = Math.Min(currentMetrics.InstanceCount + policy.ScaleUpIncrement, policy.MaxInstances);
                recommendation.Reason = "Predicted high resource utilization";
            }
            else if (prediction.CpuUtilization < policy.ScaleDownCpuThreshold && 
                     prediction.MemoryUtilization < policy.ScaleDownMemoryThreshold &&
                     prediction.RequestsPerSecond < policy.ScaleDownRpsThreshold)
            {
                recommendation.RecommendedAction = ScalingAction.ScaleDown;
                recommendation.RecommendedInstances = Math.Max(currentMetrics.InstanceCount - policy.ScaleDownDecrement, policy.MinInstances);
                recommendation.Reason = "Predicted low resource utilization";
            }
            else
            {
                recommendation.RecommendedAction = ScalingAction.NoAction;
                recommendation.RecommendedInstances = currentMetrics.InstanceCount;
                recommendation.Reason = "Predicted resource utilization within acceptable range";
            }

            return recommendation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating predictive scaling recommendation for service {ServiceName}", serviceName);
            throw;
        }
    }

    public async Task<CostOptimizationReport> GenerateCostOptimizationReportAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating cost optimization report for service {ServiceName}", serviceName);

            var currentMetrics = await GetCurrentMetricsAsync(serviceName, cancellationToken);
            var cloudProviders = await GetActiveCloudProvidersAsync(serviceName, cancellationToken);
            
            var report = new CostOptimizationReport
            {
                ServiceName = serviceName,
                GeneratedAt = DateTime.UtcNow,
                CurrentInstanceType = currentMetrics.InstanceType,
                CurrentInstanceCount = currentMetrics.InstanceCount,
                CurrentMonthlyCost = await CalculateCurrentMonthlyCostAsync(serviceName, cancellationToken)
            };

            var optimizationOpportunities = new List<CostOptimizationOpportunity>();

            foreach (var provider in cloudProviders)
            {
                var opportunities = await AnalyzeCloudProviderCostOptimizationAsync(serviceName, provider, currentMetrics, cancellationToken);
                optimizationOpportunities.AddRange(opportunities);
            }

            var bestOpportunity = optimizationOpportunities
                .OrderByDescending(o => o.PotentialSavings)
                .FirstOrDefault();

            if (bestOpportunity != null)
            {
                report.RecommendedInstanceType = bestOpportunity.RecommendedInstanceType;
                report.RecommendedCloudProvider = bestOpportunity.CloudProvider;
                report.PotentialSavings = bestOpportunity.PotentialSavings;
                report.OptimizationOpportunities = optimizationOpportunities;
            }

            report.RightsizingRecommendations = await GenerateRightsizingRecommendationsAsync(serviceName, currentMetrics, cancellationToken);

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cost optimization report for service {ServiceName}", serviceName);
            throw;
        }
    }

    public async Task<MultiCloudResourceAllocation> OptimizeMultiCloudAllocationAsync(List<string> serviceNames, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Optimizing multi-cloud allocation for {ServiceCount} services", serviceNames.Count);

            var allocation = new MultiCloudResourceAllocation
            {
                OptimizedAt = DateTime.UtcNow,
                ServiceAllocations = new Dictionary<string, CloudProviderAllocation>()
            };

            var availableProviders = await GetAvailableCloudProvidersAsync(cancellationToken);

            foreach (var serviceName in serviceNames)
            {
                var currentMetrics = await GetCurrentMetricsAsync(serviceName, cancellationToken);
                var costReports = new List<(CloudProvider Provider, decimal Cost, string InstanceType)>();

                foreach (var provider in availableProviders)
                {
                    var cost = await CalculateServiceCostOnProviderAsync(serviceName, provider, currentMetrics, cancellationToken);
                    var recommendedInstanceType = await GetOptimalInstanceTypeAsync(provider, currentMetrics, cancellationToken);
                    costReports.Add((provider, cost, recommendedInstanceType));
                }

                var optimalAllocation = costReports.OrderBy(c => c.Cost).First();
                
                allocation.ServiceAllocations[serviceName] = new CloudProviderAllocation
                {
                    ServiceName = serviceName,
                    CloudProvider = optimalAllocation.Provider.Name,
                    RecommendedInstanceType = optimalAllocation.InstanceType,
                    EstimatedMonthlyCost = optimalAllocation.Cost,
                    ReasonForSelection = "Lowest cost option with required performance characteristics"
                };
            }

            allocation.TotalEstimatedMonthlyCost = allocation.ServiceAllocations.Values.Sum(a => a.EstimatedMonthlyCost);
            allocation.EstimatedSavings = await CalculateCurrentTotalCostAsync(serviceNames, cancellationToken) - allocation.TotalEstimatedMonthlyCost;

            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, "Multi-cloud optimization completed", null, null, new { ServiceCount = serviceNames.Count, EstimatedSavings = allocation.EstimatedSavings });

            return allocation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing multi-cloud allocation");
            throw;
        }
    }

    public async Task<bool> EnablePredictiveScalingAsync(string serviceName, PredictiveScalingConfig config, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Enabling predictive scaling for service {ServiceName}", serviceName);

            _predictiveConfigs[serviceName] = config;
            
            if (!_historicalMetrics.ContainsKey(serviceName))
            {
                _historicalMetrics[serviceName] = new List<ServiceMetrics>();
            }

            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, "Predictive scaling enabled", null, null, new { ServiceName = serviceName, Config = config });
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling predictive scaling for service {ServiceName}", serviceName);
            return false;
        }
    }

    public async Task<List<CloudProviderMetrics>> GetMultiCloudMetricsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var providers = await GetAvailableCloudProvidersAsync(cancellationToken);
            var metrics = new List<CloudProviderMetrics>();

            foreach (var provider in providers)
            {
                var providerMetrics = await GetCloudProviderMetricsAsync(provider, cancellationToken);
                metrics.Add(providerMetrics);
            }

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving multi-cloud metrics");
            throw;
        }
    }

    public async Task<ResourceOptimizationSuggestions> GetResourceOptimizationSuggestionsAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating resource optimization suggestions for service {ServiceName}", serviceName);

            var currentMetrics = await GetCurrentMetricsAsync(serviceName, cancellationToken);
            var historicalData = GetHistoricalMetricsForService(serviceName);
            
            var suggestions = new ResourceOptimizationSuggestions
            {
                ServiceName = serviceName,
                GeneratedAt = DateTime.UtcNow,
                Suggestions = new List<OptimizationSuggestion>()
            };

            if (historicalData.Any() && historicalData.Average(m => m.CpuUtilization) < 30)
            {
                suggestions.Suggestions.Add(new OptimizationSuggestion
                {
                    Type = "CPU_RIGHTSIZING",
                    Priority = "High",
                    Description = "CPU utilization consistently low. Consider downsizing instance type.",
                    EstimatedSavings = await CalculateCpuRightsizingSavingsAsync(serviceName, cancellationToken),
                    ImplementationComplexity = "Low"
                });
            }

            if (historicalData.Any() && historicalData.Average(m => m.MemoryUtilization) < 40)
            {
                suggestions.Suggestions.Add(new OptimizationSuggestion
                {
                    Type = "MEMORY_RIGHTSIZING",
                    Priority = "Medium",
                    Description = "Memory utilization consistently low. Consider memory-optimized instances.",
                    EstimatedSavings = await CalculateMemoryRightsizingSavingsAsync(serviceName, cancellationToken),
                    ImplementationComplexity = "Low"
                });
            }

            var spotSavings = await CalculateSpotInstanceSavingsAsync(serviceName, cancellationToken);
            if (spotSavings > 0)
            {
                suggestions.Suggestions.Add(new OptimizationSuggestion
                {
                    Type = "SPOT_INSTANCES",
                    Priority = "Medium",
                    Description = "Consider using spot instances for non-critical workloads.",
                    EstimatedSavings = spotSavings,
                    ImplementationComplexity = "Medium"
                });
            }

            return suggestions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating resource optimization suggestions for service {ServiceName}", serviceName);
            throw;
        }
    }

    #endregion

    #region Private Helper Methods

    private async void CollectHistoricalMetrics(object? state)
    {
        try
        {
            var services = await GetAllServicesAsync();
            
            foreach (var serviceName in services)
            {
                var metrics = await GetCurrentMetricsAsync(serviceName, CancellationToken.None);
                
                if (!_historicalMetrics.ContainsKey(serviceName))
                {
                    _historicalMetrics[serviceName] = new List<ServiceMetrics>();
                }

                _historicalMetrics[serviceName].Add(metrics);

                if (_historicalMetrics[serviceName].Count > 1000)
                {
                    _historicalMetrics[serviceName].RemoveAt(0);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting historical metrics");
        }
    }

    private List<ServiceMetrics> GetHistoricalMetricsForService(string serviceName)
    {
        return _historicalMetrics.ContainsKey(serviceName) 
            ? _historicalMetrics[serviceName] 
            : new List<ServiceMetrics>();
    }

    private async Task<ResourcePrediction> PredictResourceNeedsAsync(List<ServiceMetrics> historicalData, TimeSpan forecastWindow, CancellationToken cancellationToken)
    {
        var recentData = historicalData.TakeLast(50).ToList();
        
        var avgCpuTrend = CalculateTrend(recentData.Select(m => m.CpuUtilization).ToList());
        var avgMemoryTrend = CalculateTrend(recentData.Select(m => m.MemoryUtilization).ToList());
        var avgRpsTrend = CalculateTrend(recentData.Select(m => m.RequestsPerSecond).ToList());

        var forecastMinutes = forecastWindow.TotalMinutes;
        var currentMetrics = recentData.Last();

        return await Task.FromResult(new ResourcePrediction
        {
            CpuUtilization = Math.Max(0, Math.Min(100, currentMetrics.CpuUtilization + (avgCpuTrend * forecastMinutes))),
            MemoryUtilization = Math.Max(0, Math.Min(100, currentMetrics.MemoryUtilization + (avgMemoryTrend * forecastMinutes))),
            RequestsPerSecond = Math.Max(0, currentMetrics.RequestsPerSecond + (avgRpsTrend * forecastMinutes)),
            ConfidenceScore = CalculateConfidenceScore(recentData.Count, forecastWindow)
        });
    }

    private double CalculateTrend(List<double> values)
    {
        if (values.Count < 2) return 0;

        var n = values.Count;
        var sumX = n * (n - 1) / 2.0;
        var sumY = values.Sum();
        var sumXY = values.Select((y, x) => x * y).Sum();
        var sumX2 = Enumerable.Range(0, n).Sum(x => x * x);

        return (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
    }

    private double CalculateConfidenceScore(int dataPoints, TimeSpan forecastWindow)
    {
        var baseConfidence = Math.Min(0.9, dataPoints / 100.0);
        var timeDecay = Math.Max(0.1, 1.0 - (forecastWindow.TotalHours / 24.0));
        return baseConfidence * timeDecay;
    }

    private async Task<List<CloudProvider>> GetActiveCloudProvidersAsync(string serviceName, CancellationToken cancellationToken)
    {
        return await Task.FromResult(new List<CloudProvider>
        {
            new CloudProvider { Name = "Azure", Type = CloudProviderType.Azure, IsActive = true },
            new CloudProvider { Name = "AWS", Type = CloudProviderType.AWS, IsActive = true },
            new CloudProvider { Name = "GCP", Type = CloudProviderType.GCP, IsActive = true }
        });
    }

    private async Task<bool> ScaleOnCloudProviderAsync(string serviceName, ScalingAction action, CloudProvider provider, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = provider.Type switch
            {
                CloudProviderType.Azure => _configuration["Azure:ScalingEndpoint"],
                CloudProviderType.AWS => _configuration["AWS:ScalingEndpoint"],
                CloudProviderType.GCP => _configuration["GCP:ScalingEndpoint"],
                _ => _configuration["Kubernetes:Endpoint"] ?? "http://localhost:8001"
            };

            var requestBody = new
            {
                service_name = serviceName,
                action = action.ToString(),
                provider = provider.Name,
                timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{endpoint}/api/v1/scale", content, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scaling service {ServiceName} on provider {Provider}", serviceName, provider.Name);
            return false;
        }
    }

    private async Task<List<string>> GetAllServicesAsync()
    {
        return await Task.FromResult(new List<string> { "auth-service", "chatbot-service", "analytics-service" });
    }

    private async Task<decimal> CalculateCurrentMonthlyCostAsync(string serviceName, CancellationToken cancellationToken)
    {
        return await Task.FromResult(1000m);
    }

    private async Task<List<CostOptimizationOpportunity>> AnalyzeCloudProviderCostOptimizationAsync(string serviceName, CloudProvider provider, ServiceMetrics metrics, CancellationToken cancellationToken)
    {
        return await Task.FromResult(new List<CostOptimizationOpportunity>());
    }

    private async Task<List<RightsizingRecommendation>> GenerateRightsizingRecommendationsAsync(string serviceName, ServiceMetrics metrics, CancellationToken cancellationToken)
    {
        return await Task.FromResult(new List<RightsizingRecommendation>());
    }

    private async Task<List<CloudProvider>> GetAvailableCloudProvidersAsync(CancellationToken cancellationToken)
    {
        return await GetActiveCloudProvidersAsync("", cancellationToken);
    }

    private async Task<decimal> CalculateServiceCostOnProviderAsync(string serviceName, CloudProvider provider, ServiceMetrics metrics, CancellationToken cancellationToken)
    {
        return await Task.FromResult(800m);
    }

    private async Task<string> GetOptimalInstanceTypeAsync(CloudProvider provider, ServiceMetrics metrics, CancellationToken cancellationToken)
    {
        return await Task.FromResult("Standard_D2s_v3");
    }

    private async Task<decimal> CalculateCurrentTotalCostAsync(List<string> serviceNames, CancellationToken cancellationToken)
    {
        return await Task.FromResult(serviceNames.Count * 1000m);
    }

    private async Task<CloudProviderMetrics> GetCloudProviderMetricsAsync(CloudProvider provider, CancellationToken cancellationToken)
    {
        return await Task.FromResult(new CloudProviderMetrics
        {
            ProviderName = provider.Name,
            TotalInstances = 10,
            TotalCost = 5000m,
            AverageUtilization = 65.0
        });
    }

    private async Task<decimal> CalculateCpuRightsizingSavingsAsync(string serviceName, CancellationToken cancellationToken)
    {
        return await Task.FromResult(200m);
    }

    private async Task<decimal> CalculateMemoryRightsizingSavingsAsync(string serviceName, CancellationToken cancellationToken)
    {
        return await Task.FromResult(150m);
    }

    private async Task<decimal> CalculateSpotInstanceSavingsAsync(string serviceName, CancellationToken cancellationToken)
    {
        return await Task.FromResult(300m);
    }

    #endregion

    public void Dispose()
    {
        _metricsCollectionTimer?.Dispose();
    }
}

public class ScalingDecision
{
    public string ServiceName { get; set; } = string.Empty;
    public int CurrentInstances { get; set; }
    public int TargetInstances { get; set; }
    public ScalingAction RecommendedAction { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime EvaluatedAt { get; set; }
    public PredictiveScalingRecommendation? PredictiveRecommendation { get; set; }
    public CostOptimizationReport? CostOptimization { get; set; }
    public string? RecommendedInstanceType { get; set; }
    public decimal EstimatedCostSavings { get; set; }
    public double ConfidenceScore { get; set; }
}

public class ServiceMetrics
{
    public string ServiceName { get; set; } = string.Empty;
    public int InstanceCount { get; set; }
    public double CpuUtilization { get; set; }
    public double MemoryUtilization { get; set; }
    public double RequestsPerSecond { get; set; }
    public double AverageResponseTime { get; set; }
    public double ErrorRate { get; set; }
    public DateTime LastUpdated { get; set; }
    public string InstanceType { get; set; } = string.Empty;
    public string CloudProvider { get; set; } = string.Empty;
    public decimal HourlyCost { get; set; }
}

public class ScalingPolicy
{
    public string ServiceName { get; set; } = string.Empty;
    public int MinInstances { get; set; }
    public int MaxInstances { get; set; }
    public double ScaleUpCpuThreshold { get; set; }
    public double ScaleDownCpuThreshold { get; set; }
    public double ScaleUpMemoryThreshold { get; set; }
    public double ScaleDownMemoryThreshold { get; set; }
    public int ScaleUpRpsThreshold { get; set; }
    public int ScaleDownRpsThreshold { get; set; }
    public int ScaleUpIncrement { get; set; }
    public int ScaleDownDecrement { get; set; }
    public int CooldownPeriodMinutes { get; set; }
}

public enum ScalingAction
{
    NoAction,
    ScaleUp,
    ScaleDown
}

public class PredictiveScalingRecommendation
{
    public string ServiceName { get; set; } = string.Empty;
    public ScalingAction RecommendedAction { get; set; }
    public int RecommendedInstances { get; set; }
    public string Reason { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public TimeSpan ForecastWindow { get; set; }
    public double PredictedCpuUtilization { get; set; }
    public double PredictedMemoryUtilization { get; set; }
    public double PredictedRequestsPerSecond { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class PredictiveScalingConfig
{
    public string ServiceName { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public TimeSpan ForecastWindow { get; set; } = TimeSpan.FromMinutes(30);
    public double MinConfidenceThreshold { get; set; } = 0.7;
    public int MinHistoricalDataPoints { get; set; } = 50;
    public bool EnableCostOptimization { get; set; } = true;
}

public class CostOptimizationReport
{
    public string ServiceName { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string CurrentInstanceType { get; set; } = string.Empty;
    public int CurrentInstanceCount { get; set; }
    public decimal CurrentMonthlyCost { get; set; }
    public string? RecommendedInstanceType { get; set; }
    public string? RecommendedCloudProvider { get; set; }
    public decimal PotentialSavings { get; set; }
    public List<CostOptimizationOpportunity> OptimizationOpportunities { get; set; } = new();
    public List<RightsizingRecommendation> RightsizingRecommendations { get; set; } = new();
}

public class CostOptimizationOpportunity
{
    public string CloudProvider { get; set; } = string.Empty;
    public string RecommendedInstanceType { get; set; } = string.Empty;
    public decimal PotentialSavings { get; set; }
    public double SavingsPercentage { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ImplementationComplexity { get; set; } = string.Empty;
}

public class RightsizingRecommendation
{
    public string RecommendationType { get; set; } = string.Empty;
    public string CurrentConfiguration { get; set; } = string.Empty;
    public string RecommendedConfiguration { get; set; } = string.Empty;
    public decimal EstimatedSavings { get; set; }
    public string Justification { get; set; } = string.Empty;
}

public class MultiCloudResourceAllocation
{
    public DateTime OptimizedAt { get; set; }
    public Dictionary<string, CloudProviderAllocation> ServiceAllocations { get; set; } = new();
    public decimal TotalEstimatedMonthlyCost { get; set; }
    public decimal EstimatedSavings { get; set; }
    public string OptimizationStrategy { get; set; } = "Cost-Optimized";
}

public class CloudProviderAllocation
{
    public string ServiceName { get; set; } = string.Empty;
    public string CloudProvider { get; set; } = string.Empty;
    public string RecommendedInstanceType { get; set; } = string.Empty;
    public decimal EstimatedMonthlyCost { get; set; }
    public string ReasonForSelection { get; set; } = string.Empty;
}

public class CloudProvider
{
    public string Name { get; set; } = string.Empty;
    public CloudProviderType Type { get; set; }
    public bool IsActive { get; set; }
    public Dictionary<string, string> Configuration { get; set; } = new();
}

public enum CloudProviderType
{
    Azure,
    AWS,
    GCP,
    OnPremise
}

public class CloudProviderMetrics
{
    public string ProviderName { get; set; } = string.Empty;
    public int TotalInstances { get; set; }
    public decimal TotalCost { get; set; }
    public double AverageUtilization { get; set; }
    public Dictionary<string, double> RegionalMetrics { get; set; } = new();
}

public class ResourceOptimizationSuggestions
{
    public string ServiceName { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public List<OptimizationSuggestion> Suggestions { get; set; } = new();
    public decimal TotalPotentialSavings { get; set; }
}

public class OptimizationSuggestion
{
    public string Type { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal EstimatedSavings { get; set; }
    public string ImplementationComplexity { get; set; } = string.Empty;
    public List<string> RequiredActions { get; set; } = new();
}

public class ResourcePrediction
{
    public double CpuUtilization { get; set; }
    public double MemoryUtilization { get; set; }
    public double RequestsPerSecond { get; set; }
    public double ConfidenceScore { get; set; }
    public DateTime PredictedAt { get; set; } = DateTime.UtcNow;
}
