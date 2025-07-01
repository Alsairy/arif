using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Arif.Platform.Analytics.Domain.Interfaces;
using Arif.Platform.Analytics.Domain.DTOs;
using Arif.Platform.Shared.Infrastructure.Services;

namespace Arif.Platform.Analytics.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAdvancedAnalyticsService _analyticsService;
        private readonly IDashboardService _dashboardService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(
            IAdvancedAnalyticsService analyticsService,
            IDashboardService dashboardService,
            ICurrentUserService currentUserService,
            ILogger<AnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _dashboardService = dashboardService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversationAnalyticsAsync(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? botId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var tenantId = _currentUserService.TenantId ?? throw new UnauthorizedAccessException("Tenant ID not found");
                var request = new ConversationAnalyticsRequest
                {
                    TenantId = tenantId,
                    StartDate = startDate ?? DateTime.UtcNow.AddDays(-30),
                    EndDate = endDate ?? DateTime.UtcNow,
                    BotId = botId,
                    Page = page,
                    PageSize = pageSize
                };

                _logger.LogInformation("Retrieving conversation analytics for tenant {TenantId}", tenantId);

                var analytics = await _analyticsService.GetConversationAnalyticsAsync(request);
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conversation analytics");
                return StatusCode(500, new { error = "An error occurred while retrieving conversation analytics" });
            }
        }

        [HttpGet("engagement")]
        public async Task<IActionResult> GetUserEngagementMetricsAsync(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? userId = null)
        {
            try
            {
                var tenantId = _currentUserService.TenantId ?? throw new UnauthorizedAccessException("Tenant ID not found");
                var request = new UserEngagementRequest
                {
                    TenantId = tenantId,
                    StartDate = startDate ?? DateTime.UtcNow.AddDays(-30),
                    EndDate = endDate ?? DateTime.UtcNow,
                    UserId = userId
                };

                _logger.LogInformation("Retrieving user engagement metrics for tenant {TenantId}", tenantId);

                var metrics = await _analyticsService.GetUserEngagementMetricsAsync(request);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user engagement metrics");
                return StatusCode(500, new { error = "An error occurred while retrieving user engagement metrics" });
            }
        }

        [HttpGet("performance")]
        public async Task<IActionResult> GetBotPerformanceTrackingAsync(
            [FromQuery] string? botId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var tenantId = _currentUserService.TenantId ?? throw new UnauthorizedAccessException("Tenant ID not found");
                var request = new BotPerformanceRequest
                {
                    TenantId = tenantId,
                    BotId = botId,
                    StartDate = startDate ?? DateTime.UtcNow.AddDays(-7),
                    EndDate = endDate ?? DateTime.UtcNow
                };

                _logger.LogInformation("Retrieving bot performance tracking for tenant {TenantId}", tenantId);

                var performance = await _analyticsService.GetBotPerformanceTrackingAsync(request);
                return Ok(performance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bot performance tracking");
                return StatusCode(500, new { error = "An error occurred while retrieving bot performance tracking" });
            }
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetRealTimeDashboardDataAsync()
        {
            try
            {
                var tenantId = _currentUserService.TenantId ?? throw new UnauthorizedAccessException("Tenant ID not found");

                _logger.LogInformation("Retrieving real-time dashboard data for tenant {TenantId}", tenantId);

                var dashboardData = await _dashboardService.GetRealTimeDashboardDataAsync(tenantId);
                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving real-time dashboard data");
                return StatusCode(500, new { error = "An error occurred while retrieving dashboard data" });
            }
        }

        [HttpGet("insights")]
        public async Task<IActionResult> GetPredictiveInsightsAsync(
            [FromQuery] string insightType = "general",
            [FromQuery] int forecastDays = 7)
        {
            try
            {
                var tenantId = _currentUserService.TenantId ?? throw new UnauthorizedAccessException("Tenant ID not found");
                var request = new PredictiveInsightsRequest
                {
                    TenantId = tenantId,
                    InsightType = insightType,
                    ForecastDays = forecastDays
                };

                _logger.LogInformation("Retrieving predictive insights for tenant {TenantId}", tenantId);

                var insights = await _analyticsService.GetPredictiveInsightsAsync(request);
                return Ok(insights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving predictive insights");
                return StatusCode(500, new { error = "An error occurred while retrieving predictive insights" });
            }
        }

        [HttpGet("anomalies")]
        public async Task<IActionResult> GetAnomalyDetectionAsync(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var tenantId = _currentUserService.TenantId ?? throw new UnauthorizedAccessException("Tenant ID not found");
                var request = new AnomalyDetectionRequest
                {
                    TenantId = tenantId,
                    StartDate = startDate ?? DateTime.UtcNow.AddDays(-7),
                    EndDate = endDate ?? DateTime.UtcNow
                };

                _logger.LogInformation("Retrieving anomaly detection for tenant {TenantId}", tenantId);

                var anomalies = await _analyticsService.GetAnomalyDetectionAsync(request);
                return Ok(anomalies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving anomaly detection");
                return StatusCode(500, new { error = "An error occurred while retrieving anomaly detection" });
            }
        }

        [HttpPost("events")]
        public async Task<IActionResult> TrackEventAsync([FromBody] AnalyticsEventRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.EventType))
                {
                    return BadRequest(new { error = "EventType is required" });
                }

                var tenantId = _currentUserService.TenantId ?? throw new UnauthorizedAccessException("Tenant ID not found");
                var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User ID not found");

                request.TenantId = tenantId;
                request.UserId = userId;
                request.Timestamp = DateTime.UtcNow;

                _logger.LogInformation("Tracking analytics event {EventType} for user {UserId} in tenant {TenantId}", 
                    request.EventType, userId, tenantId);

                await _analyticsService.TrackEventAsync(request);
                return Ok(new { message = "Event tracked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking analytics event");
                return StatusCode(500, new { error = "An error occurred while tracking the event" });
            }
        }

        [HttpGet("reports/{reportId}")]
        public async Task<IActionResult> GetAnalyticsReportAsync(string reportId)
        {
            try
            {
                var tenantId = _currentUserService.TenantId ?? throw new UnauthorizedAccessException("Tenant ID not found");

                _logger.LogInformation("Retrieving analytics report {ReportId} for tenant {TenantId}", reportId, tenantId);

                var report = await _analyticsService.GetAnalyticsReportAsync(reportId, tenantId);
                
                if (report == null)
                {
                    return NotFound(new { error = "Report not found" });
                }

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving analytics report {ReportId}", reportId);
                return StatusCode(500, new { error = "An error occurred while retrieving the report" });
            }
        }

        [HttpPost("reports")]
        public async Task<IActionResult> GenerateCustomReportAsync([FromBody] CustomReportRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.ReportName))
                {
                    return BadRequest(new { error = "ReportName is required" });
                }

                var tenantId = _currentUserService.TenantId ?? throw new UnauthorizedAccessException("Tenant ID not found");
                var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User ID not found");

                request.TenantId = tenantId;
                request.CreatedBy = userId;

                _logger.LogInformation("Generating custom report {ReportName} for user {UserId} in tenant {TenantId}", 
                    request.ReportName, userId, tenantId);

                var report = await _analyticsService.GenerateCustomReportAsync(request);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating custom report");
                return StatusCode(500, new { error = "An error occurred while generating the report" });
            }
        }

        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                service = "Analytics Service",
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }
    }
}
