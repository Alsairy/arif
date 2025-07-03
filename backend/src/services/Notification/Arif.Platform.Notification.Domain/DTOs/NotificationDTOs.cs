using System.ComponentModel.DataAnnotations;

namespace Arif.Platform.Notification.Domain.DTOs
{
    public class SendNotificationRequest
    {
        [Required]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        public string Channel { get; set; } = string.Empty;
        
        public string[] Recipients { get; set; } = Array.Empty<string>();
        
        public Guid TenantId { get; set; }
        
        public Guid SentBy { get; set; }
        
        public string? Subject { get; set; }
        
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        
        public string? TemplateId { get; set; }
        
        public Dictionary<string, object>? TemplateData { get; set; }
        
        public DateTime? ScheduledAt { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class NotificationResponse
    {
        public Guid Id { get; set; }
        
        public string Message { get; set; } = string.Empty;
        
        public string Channel { get; set; } = string.Empty;
        
        public string[] Recipients { get; set; } = Array.Empty<string>();
        
        public Guid TenantId { get; set; }
        
        public string Status { get; set; } = string.Empty;
        
        public NotificationPriority Priority { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? SentAt { get; set; }
        
        public DateTime? DeliveredAt { get; set; }
        
        public string? ErrorMessage { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public enum NotificationPriority
    {
        Low,
        Normal,
        High,
        Critical
    }

    public class SendEmailRequest
    {
        [Required]
        [EmailAddress]
        public string To { get; set; } = string.Empty;
        
        public string[]? Cc { get; set; }
        
        public string[]? Bcc { get; set; }
        
        [Required]
        public string Subject { get; set; } = string.Empty;
        
        [Required]
        public string Body { get; set; } = string.Empty;
        
        public bool IsHtml { get; set; } = true;
        
        public Guid TenantId { get; set; }
        
        public Guid SentBy { get; set; }
        
        public string? TemplateId { get; set; }
        
        public Dictionary<string, object>? TemplateData { get; set; }
        
        public EmailAttachment[] Attachments { get; set; } = Array.Empty<EmailAttachment>();
        
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class EmailAttachment
    {
        public string FileName { get; set; } = string.Empty;
        
        public byte[] Content { get; set; } = Array.Empty<byte>();
        
        public string ContentType { get; set; } = string.Empty;
    }

    public class EmailResponse
    {
        public Guid Id { get; set; }
        
        public string To { get; set; } = string.Empty;
        
        public string Subject { get; set; } = string.Empty;
        
        public string Status { get; set; } = string.Empty;
        
        public DateTime SentAt { get; set; }
        
        public string? MessageId { get; set; }
        
        public string? ErrorMessage { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class SendSmsRequest
    {
        [Required]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        public Guid TenantId { get; set; }
        
        public Guid SentBy { get; set; }
        
        public string? TemplateId { get; set; }
        
        public Dictionary<string, object>? TemplateData { get; set; }
        
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class SmsResponse
    {
        public Guid Id { get; set; }
        
        public string PhoneNumber { get; set; } = string.Empty;
        
        public string Message { get; set; } = string.Empty;
        
        public string Status { get; set; } = string.Empty;
        
        public DateTime SentAt { get; set; }
        
        public string? MessageId { get; set; }
        
        public string? ErrorMessage { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class SendPushNotificationRequest
    {
        [Required]
        public string DeviceToken { get; set; } = string.Empty;
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Body { get; set; } = string.Empty;
        
        public Guid TenantId { get; set; }
        
        public Guid SentBy { get; set; }
        
        public string? Icon { get; set; }
        
        public string? Image { get; set; }
        
        public string? Sound { get; set; }
        
        public string? ClickAction { get; set; }
        
        public Dictionary<string, object>? Data { get; set; }
        
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class PushNotificationResponse
    {
        public Guid Id { get; set; }
        
        public string DeviceToken { get; set; } = string.Empty;
        
        public string Title { get; set; } = string.Empty;
        
        public string Body { get; set; } = string.Empty;
        
        public string Status { get; set; } = string.Empty;
        
        public DateTime SentAt { get; set; }
        
        public string? MessageId { get; set; }
        
        public string? ErrorMessage { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class GetNotificationHistoryRequest
    {
        public Guid TenantId { get; set; }
        
        public string? Channel { get; set; }
        
        public string? Status { get; set; }
        
        public string? Recipient { get; set; }
        
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        public int Page { get; set; } = 1;
        
        public int PageSize { get; set; } = 20;
    }

    public class GetNotificationHistoryResponse
    {
        public NotificationHistoryItem[] Items { get; set; } = Array.Empty<NotificationHistoryItem>();
        
        public int TotalCount { get; set; }
        
        public int Page { get; set; }
        
        public int PageSize { get; set; }
        
        public int TotalPages { get; set; }
    }

    public class NotificationHistoryItem
    {
        public Guid Id { get; set; }
        
        public string Channel { get; set; } = string.Empty;
        
        public string Recipient { get; set; } = string.Empty;
        
        public string Subject { get; set; } = string.Empty;
        
        public string Message { get; set; } = string.Empty;
        
        public string Status { get; set; } = string.Empty;
        
        public NotificationPriority Priority { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? SentAt { get; set; }
        
        public DateTime? DeliveredAt { get; set; }
        
        public string? ErrorMessage { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class CreateTemplateRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        [Required]
        public string Type { get; set; } = string.Empty;
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        public string? Subject { get; set; }
        
        public Guid TenantId { get; set; }
        
        public Guid CreatedBy { get; set; }
        
        public string Language { get; set; } = "ar";
        
        public TemplateVariable[] Variables { get; set; } = Array.Empty<TemplateVariable>();
        
        public bool IsActive { get; set; } = true;
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class TemplateVariable
    {
        public string Name { get; set; } = string.Empty;
        
        public string Type { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public bool IsRequired { get; set; }
        
        public string? DefaultValue { get; set; }
    }

    public class TemplateResponse
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public string Type { get; set; } = string.Empty;
        
        public string Content { get; set; } = string.Empty;
        
        public string? Subject { get; set; }
        
        public Guid TenantId { get; set; }
        
        public string Language { get; set; } = string.Empty;
        
        public TemplateVariable[] Variables { get; set; } = Array.Empty<TemplateVariable>();
        
        public bool IsActive { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class UpdateTemplateRequest
    {
        public Guid TemplateId { get; set; }
        
        public Guid TenantId { get; set; }
        
        public Guid UpdatedBy { get; set; }
        
        public string? Name { get; set; }
        
        public string? Description { get; set; }
        
        public string? Content { get; set; }
        
        public string? Subject { get; set; }
        
        public TemplateVariable[]? Variables { get; set; }
        
        public bool? IsActive { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class GetNotificationStatisticsRequest
    {
        public Guid TenantId { get; set; }
        
        public DateTime StartDate { get; set; }
        
        public DateTime EndDate { get; set; }
        
        public string[]? Channels { get; set; }
        
        public string Granularity { get; set; } = "daily";
    }

    public class NotificationStatisticsResponse
    {
        public NotificationStatistic[] Statistics { get; set; } = Array.Empty<NotificationStatistic>();
        
        public NotificationSummary Summary { get; set; } = new();
        
        public DateTime PeriodStart { get; set; }
        
        public DateTime PeriodEnd { get; set; }
    }

    public class NotificationStatistic
    {
        public string Channel { get; set; } = string.Empty;
        
        public DateTime Date { get; set; }
        
        public int Sent { get; set; }
        
        public int Delivered { get; set; }
        
        public int Failed { get; set; }
        
        public double DeliveryRate { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class NotificationSummary
    {
        public int TotalSent { get; set; }
        
        public int TotalDelivered { get; set; }
        
        public int TotalFailed { get; set; }
        
        public double OverallDeliveryRate { get; set; }
        
        public Dictionary<string, int> ByChannel { get; set; } = new();
        
        public Dictionary<string, int> ByStatus { get; set; } = new();
    }

    public class NotificationHub
    {
        public string ConnectionId { get; set; } = string.Empty;
        
        public Guid? UserId { get; set; }
        
        public Guid TenantId { get; set; }
        
        public DateTime ConnectedAt { get; set; }
        
        public Dictionary<string, object>? Context { get; set; }
    }

    public class RealTimeNotificationEvent
    {
        public string EventType { get; set; } = string.Empty;
        
        public Guid NotificationId { get; set; }
        
        public string Channel { get; set; } = string.Empty;
        
        public string Status { get; set; } = string.Empty;
        
        public Dictionary<string, object>? Data { get; set; }
        
        public DateTime Timestamp { get; set; }
    }

    public class NotificationDeliveryStatus
    {
        public Guid NotificationId { get; set; }
        
        public string Status { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; }
        
        public string? ErrorMessage { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class BulkNotificationRequest
    {
        [Required]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        public string Channel { get; set; } = string.Empty;
        
        [Required]
        public string[] Recipients { get; set; } = Array.Empty<string>();
        
        public Guid TenantId { get; set; }
        
        public Guid SentBy { get; set; }
        
        public string? Subject { get; set; }
        
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        
        public string? TemplateId { get; set; }
        
        public Dictionary<string, Dictionary<string, object>>? RecipientTemplateData { get; set; }
        
        public DateTime? ScheduledAt { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class BulkNotificationResponse
    {
        public Guid BatchId { get; set; }
        
        public int TotalRecipients { get; set; }
        
        public int SuccessCount { get; set; }
        
        public int FailureCount { get; set; }
        
        public NotificationResponse[] Results { get; set; } = Array.Empty<NotificationResponse>();
        
        public DateTime ProcessedAt { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
