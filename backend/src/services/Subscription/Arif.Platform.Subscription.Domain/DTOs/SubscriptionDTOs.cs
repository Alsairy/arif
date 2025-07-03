using System.ComponentModel.DataAnnotations;

namespace Arif.Platform.Subscription.Domain.DTOs
{
    public class SubscriptionPlan
    {
        public string Id { get; set; } = string.Empty;
        
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public decimal Price { get; set; }
        
        public string Currency { get; set; } = "USD";
        
        public string BillingInterval { get; set; } = "monthly";
        
        public SubscriptionFeature[] Features { get; set; } = Array.Empty<SubscriptionFeature>();
        
        public SubscriptionLimit[] Limits { get; set; } = Array.Empty<SubscriptionLimit>();
        
        public bool IsActive { get; set; } = true;
        
        public bool IsPopular { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class SubscriptionFeature
    {
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public bool IsIncluded { get; set; }
        
        public string? Value { get; set; }
    }

    public class SubscriptionLimit
    {
        public string Type { get; set; } = string.Empty;
        
        public int Limit { get; set; }
        
        public string Period { get; set; } = "monthly";
        
        public string Description { get; set; } = string.Empty;
    }

    public class CreateSubscriptionRequest
    {
        [Required]
        public string PlanId { get; set; } = string.Empty;
        
        public Guid TenantId { get; set; }
        
        public Guid CreatedBy { get; set; }
        
        public string? PaymentMethodId { get; set; }
        
        public DateTime? StartDate { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class SubscriptionResponse
    {
        public Guid Id { get; set; }
        
        public string PlanId { get; set; } = string.Empty;
        
        public string PlanName { get; set; } = string.Empty;
        
        public Guid TenantId { get; set; }
        
        public string Status { get; set; } = string.Empty;
        
        public DateTime StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        public DateTime? NextBillingDate { get; set; }
        
        public decimal Amount { get; set; }
        
        public string Currency { get; set; } = string.Empty;
        
        public string BillingInterval { get; set; } = string.Empty;
        
        public SubscriptionUsage CurrentUsage { get; set; } = new();
        
        public Dictionary<string, object>? Metadata { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
    }

    public class SubscriptionUsage
    {
        public UsageMetric[] Metrics { get; set; } = Array.Empty<UsageMetric>();
        
        public DateTime PeriodStart { get; set; }
        
        public DateTime PeriodEnd { get; set; }
        
        public Dictionary<string, object>? Summary { get; set; }
    }

    public class UsageMetric
    {
        public string Type { get; set; } = string.Empty;
        
        public int Used { get; set; }
        
        public int Limit { get; set; }
        
        public string Unit { get; set; } = string.Empty;
        
        public double Percentage { get; set; }
        
        public bool IsOverLimit { get; set; }
    }

    public class UpgradeSubscriptionRequest
    {
        [Required]
        public string NewPlanId { get; set; } = string.Empty;
        
        public Guid TenantId { get; set; }
        
        public Guid UpdatedBy { get; set; }
        
        public bool ProrateBilling { get; set; } = true;
        
        public DateTime? EffectiveDate { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class CancelSubscriptionRequest
    {
        public Guid TenantId { get; set; }
        
        public Guid CancelledBy { get; set; }
        
        public string? Reason { get; set; }
        
        public bool CancelImmediately { get; set; } = false;
        
        public DateTime? CancellationDate { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class GetUsageStatisticsRequest
    {
        public Guid TenantId { get; set; }
        
        public DateTime StartDate { get; set; }
        
        public DateTime EndDate { get; set; }
        
        public string[]? MetricTypes { get; set; }
        
        public string Granularity { get; set; } = "daily";
    }

    public class UsageStatisticsResponse
    {
        public UsageStatistic[] Statistics { get; set; } = Array.Empty<UsageStatistic>();
        
        public DateTime PeriodStart { get; set; }
        
        public DateTime PeriodEnd { get; set; }
        
        public UsageSummary Summary { get; set; } = new();
    }

    public class UsageStatistic
    {
        public string MetricType { get; set; } = string.Empty;
        
        public DateTime Date { get; set; }
        
        public int Value { get; set; }
        
        public string Unit { get; set; } = string.Empty;
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class UsageSummary
    {
        public int TotalUsage { get; set; }
        
        public int AverageDaily { get; set; }
        
        public int PeakDaily { get; set; }
        
        public Dictionary<string, int> ByMetricType { get; set; } = new();
    }

    public class TrackUsageRequest
    {
        [Required]
        public string MetricType { get; set; } = string.Empty;
        
        public Guid TenantId { get; set; }
        
        public Guid UserId { get; set; }
        
        public int Quantity { get; set; } = 1;
        
        public DateTime Timestamp { get; set; }
        
        public Dictionary<string, object>? Properties { get; set; }
    }

    public class GetInvoicesRequest
    {
        public Guid TenantId { get; set; }
        
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        public string? Status { get; set; }
        
        public int Page { get; set; } = 1;
        
        public int PageSize { get; set; } = 20;
    }

    public class GetInvoicesResponse
    {
        public Invoice[] Items { get; set; } = Array.Empty<Invoice>();
        
        public int TotalCount { get; set; }
        
        public int Page { get; set; }
        
        public int PageSize { get; set; }
        
        public int TotalPages { get; set; }
    }

    public class Invoice
    {
        public Guid Id { get; set; }
        
        public string InvoiceNumber { get; set; } = string.Empty;
        
        public Guid TenantId { get; set; }
        
        public Guid SubscriptionId { get; set; }
        
        public string Status { get; set; } = string.Empty;
        
        public decimal Amount { get; set; }
        
        public decimal Tax { get; set; }
        
        public decimal Total { get; set; }
        
        public string Currency { get; set; } = string.Empty;
        
        public DateTime IssueDate { get; set; }
        
        public DateTime DueDate { get; set; }
        
        public DateTime? PaidDate { get; set; }
        
        public InvoiceLineItem[] LineItems { get; set; } = Array.Empty<InvoiceLineItem>();
        
        public string? PaymentMethodId { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class InvoiceLineItem
    {
        public string Description { get; set; } = string.Empty;
        
        public int Quantity { get; set; }
        
        public decimal UnitPrice { get; set; }
        
        public decimal Amount { get; set; }
        
        public string? MetricType { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class AddPaymentMethodRequest
    {
        [Required]
        public string PaymentToken { get; set; } = string.Empty;
        
        public Guid TenantId { get; set; }
        
        public Guid AddedBy { get; set; }
        
        public string Type { get; set; } = "card";
        
        public bool SetAsDefault { get; set; } = false;
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class PaymentMethod
    {
        public Guid Id { get; set; }
        
        public Guid TenantId { get; set; }
        
        public string Type { get; set; } = string.Empty;
        
        public string Last4 { get; set; } = string.Empty;
        
        public string Brand { get; set; } = string.Empty;
        
        public int ExpiryMonth { get; set; }
        
        public int ExpiryYear { get; set; }
        
        public bool IsDefault { get; set; }
        
        public bool IsActive { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class BillingAddress
    {
        public string Line1 { get; set; } = string.Empty;
        
        public string? Line2 { get; set; }
        
        public string City { get; set; } = string.Empty;
        
        public string State { get; set; } = string.Empty;
        
        public string PostalCode { get; set; } = string.Empty;
        
        public string Country { get; set; } = string.Empty;
    }

    public class SubscriptionEvent
    {
        public Guid Id { get; set; }
        
        public Guid SubscriptionId { get; set; }
        
        public string EventType { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; }
        
        public Dictionary<string, object>? Data { get; set; }
        
        public Guid? UserId { get; set; }
    }

    public class SubscriptionAnalytics
    {
        public int TotalSubscriptions { get; set; }
        
        public int ActiveSubscriptions { get; set; }
        
        public int CancelledSubscriptions { get; set; }
        
        public decimal MonthlyRecurringRevenue { get; set; }
        
        public decimal AverageRevenuePerUser { get; set; }
        
        public double ChurnRate { get; set; }
        
        public Dictionary<string, int> SubscriptionsByPlan { get; set; } = new();
        
        public Dictionary<string, decimal> RevenueByPlan { get; set; } = new();
        
        public DateTime PeriodStart { get; set; }
        
        public DateTime PeriodEnd { get; set; }
    }
}
