using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Arif.Platform.Subscription.Domain.Interfaces;
using Arif.Platform.Subscription.Domain.DTOs;
using Arif.Platform.Shared.Infrastructure.Services;

namespace Arif.Platform.Subscription.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly IBillingService _billingService;
        private readonly IUsageTrackingService _usageTrackingService;
        private readonly IPlanManagementService _planManagementService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<SubscriptionController> _logger;

        public SubscriptionController(
            ISubscriptionService subscriptionService,
            IBillingService billingService,
            IUsageTrackingService usageTrackingService,
            IPlanManagementService planManagementService,
            ICurrentUserService currentUserService,
            ILogger<SubscriptionController> logger)
        {
            _subscriptionService = subscriptionService;
            _billingService = billingService;
            _usageTrackingService = usageTrackingService;
            _planManagementService = planManagementService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        [HttpGet("plans")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSubscriptionPlansAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving available subscription plans");

                var plans = await _planManagementService.GetAvailablePlansAsync();
                return Ok(plans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subscription plans");
                return StatusCode(500, new { error = "An error occurred while retrieving subscription plans" });
            }
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentSubscriptionAsync()
        {
            try
            {
                var tenantId = _currentUserService.TenantId;

                _logger.LogInformation("Retrieving current subscription for tenant {TenantId}", tenantId);

                var subscription = await _subscriptionService.GetCurrentSubscriptionAsync(tenantId.Value);
                
                if (subscription == null)
                {
                    return NotFound(new { error = "No active subscription found" });
                }

                return Ok(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current subscription");
                return StatusCode(500, new { error = "An error occurred while retrieving the current subscription" });
            }
        }

        [HttpPost("subscribe")]
        public async Task<IActionResult> CreateSubscriptionAsync([FromBody] CreateSubscriptionRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.PlanId))
                {
                    return BadRequest(new { error = "PlanId is required" });
                }

                var tenantId = _currentUserService.TenantId;
                var userId = _currentUserService.UserId;

                request.TenantId = tenantId.Value;
                request.CreatedBy = userId.Value;

                _logger.LogInformation("Creating subscription for plan {PlanId} for tenant {TenantId}", 
                    request.PlanId, tenantId);

                var subscription = await _subscriptionService.CreateSubscriptionAsync(request);
                return Ok(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subscription");
                return StatusCode(500, new { error = "An error occurred while creating the subscription" });
            }
        }

        [HttpPut("upgrade")]
        public async Task<IActionResult> UpgradeSubscriptionAsync([FromBody] UpgradeSubscriptionRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.NewPlanId))
                {
                    return BadRequest(new { error = "NewPlanId is required" });
                }

                var tenantId = _currentUserService.TenantId;
                var userId = _currentUserService.UserId;

                request.TenantId = tenantId.Value;
                request.UpdatedBy = userId.Value;

                _logger.LogInformation("Upgrading subscription to plan {NewPlanId} for tenant {TenantId}", 
                    request.NewPlanId, tenantId);

                var subscription = await _subscriptionService.UpgradeSubscriptionAsync(request);
                return Ok(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upgrading subscription");
                return StatusCode(500, new { error = "An error occurred while upgrading the subscription" });
            }
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> CancelSubscriptionAsync([FromBody] CancelSubscriptionRequest request)
        {
            try
            {
                var tenantId = _currentUserService.TenantId;
                var userId = _currentUserService.UserId;

                request.TenantId = tenantId.Value;
                request.CancelledBy = userId.Value;

                _logger.LogInformation("Cancelling subscription for tenant {TenantId}", tenantId);

                var success = await _subscriptionService.CancelSubscriptionAsync(request);
                
                if (!success)
                {
                    return BadRequest(new { error = "Unable to cancel subscription" });
                }

                return Ok(new { message = "Subscription cancelled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling subscription");
                return StatusCode(500, new { error = "An error occurred while cancelling the subscription" });
            }
        }

        [HttpGet("usage")]
        public async Task<IActionResult> GetUsageStatisticsAsync(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var tenantId = _currentUserService.TenantId;

                var request = new GetUsageStatisticsRequest
                {
                    TenantId = tenantId.Value,
                    StartDate = startDate ?? DateTime.UtcNow.AddDays(-30),
                    EndDate = endDate ?? DateTime.UtcNow
                };

                _logger.LogInformation("Retrieving usage statistics for tenant {TenantId}", tenantId);

                var usage = await _usageTrackingService.GetUsageStatisticsAsync(request);
                return Ok(usage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving usage statistics");
                return StatusCode(500, new { error = "An error occurred while retrieving usage statistics" });
            }
        }

        [HttpPost("usage/track")]
        public async Task<IActionResult> TrackUsageAsync([FromBody] TrackUsageRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.MetricType))
                {
                    return BadRequest(new { error = "MetricType is required" });
                }

                var tenantId = _currentUserService.TenantId;
                var userId = _currentUserService.UserId;

                request.TenantId = tenantId.Value;
                request.UserId = userId.Value;
                request.Timestamp = DateTime.UtcNow;

                _logger.LogInformation("Tracking usage metric {MetricType} for tenant {TenantId}", 
                    request.MetricType, tenantId);

                await _usageTrackingService.TrackUsageAsync(request);
                return Ok(new { message = "Usage tracked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking usage");
                return StatusCode(500, new { error = "An error occurred while tracking usage" });
            }
        }

        [HttpGet("billing/invoices")]
        public async Task<IActionResult> GetInvoicesAsync(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var tenantId = _currentUserService.TenantId;

                var request = new GetInvoicesRequest
                {
                    TenantId = tenantId.Value,
                    Page = page,
                    PageSize = pageSize
                };

                _logger.LogInformation("Retrieving invoices for tenant {TenantId}", tenantId);

                var invoices = await _billingService.GetInvoicesAsync(request);
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoices");
                return StatusCode(500, new { error = "An error occurred while retrieving invoices" });
            }
        }

        [HttpGet("billing/invoices/{invoiceId}")]
        public async Task<IActionResult> GetInvoiceAsync(Guid invoiceId)
        {
            try
            {
                var tenantId = _currentUserService.TenantId;

                _logger.LogInformation("Retrieving invoice {InvoiceId} for tenant {TenantId}", invoiceId, tenantId);

                var invoice = await _billingService.GetInvoiceAsync(invoiceId, tenantId.Value);
                
                if (invoice == null)
                {
                    return NotFound(new { error = "Invoice not found" });
                }

                return Ok(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoice {InvoiceId}", invoiceId);
                return StatusCode(500, new { error = "An error occurred while retrieving the invoice" });
            }
        }

        [HttpPost("billing/payment-methods")]
        public async Task<IActionResult> AddPaymentMethodAsync([FromBody] AddPaymentMethodRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.PaymentToken))
                {
                    return BadRequest(new { error = "PaymentToken is required" });
                }

                var tenantId = _currentUserService.TenantId;
                var userId = _currentUserService.UserId;

                request.TenantId = tenantId.Value;
                request.AddedBy = userId.Value;

                _logger.LogInformation("Adding payment method for tenant {TenantId}", tenantId);

                var paymentMethod = await _billingService.AddPaymentMethodAsync(request);
                return Ok(paymentMethod);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding payment method");
                return StatusCode(500, new { error = "An error occurred while adding the payment method" });
            }
        }

        [HttpGet("billing/payment-methods")]
        public async Task<IActionResult> GetPaymentMethodsAsync()
        {
            try
            {
                var tenantId = _currentUserService.TenantId;

                _logger.LogInformation("Retrieving payment methods for tenant {TenantId}", tenantId);

                var paymentMethods = await _billingService.GetPaymentMethodsAsync(tenantId.Value);
                return Ok(paymentMethods);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment methods");
                return StatusCode(500, new { error = "An error occurred while retrieving payment methods" });
            }
        }

        [HttpDelete("billing/payment-methods/{paymentMethodId}")]
        public async Task<IActionResult> RemovePaymentMethodAsync(Guid paymentMethodId)
        {
            try
            {
                var tenantId = _currentUserService.TenantId;

                _logger.LogInformation("Removing payment method {PaymentMethodId} for tenant {TenantId}", 
                    paymentMethodId, tenantId);

                var success = await _billingService.RemovePaymentMethodAsync(paymentMethodId, tenantId.Value);
                
                if (!success)
                {
                    return NotFound(new { error = "Payment method not found" });
                }

                return Ok(new { message = "Payment method removed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing payment method {PaymentMethodId}", paymentMethodId);
                return StatusCode(500, new { error = "An error occurred while removing the payment method" });
            }
        }

        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                service = "Subscription Service",
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }
    }
}
