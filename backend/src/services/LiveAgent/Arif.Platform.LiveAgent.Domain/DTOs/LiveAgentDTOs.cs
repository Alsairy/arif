using System.ComponentModel.DataAnnotations;

namespace Arif.Platform.LiveAgent.Domain.DTOs
{
    public class GetAgentsRequest
    {
        public Guid TenantId { get; set; }
        
        public string? Status { get; set; }
        
        public string? Department { get; set; }
        
        public string? Skill { get; set; }
        
        public int Page { get; set; } = 1;
        
        public int PageSize { get; set; } = 20;
    }

    public class GetAgentsResponse
    {
        public AgentResponse[] Agents { get; set; } = Array.Empty<AgentResponse>();
        
        public int TotalCount { get; set; }
        
        public int Page { get; set; }
        
        public int PageSize { get; set; }
        
        public int TotalPages { get; set; }
    }

    public class CreateAgentRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        public string LastName { get; set; } = string.Empty;
        
        public string? PhoneNumber { get; set; }
        
        public string? Department { get; set; }
        
        public string[] Skills { get; set; } = Array.Empty<string>();
        
        public string[] Languages { get; set; } = Array.Empty<string>();
        
        public AgentRole Role { get; set; } = AgentRole.Agent;
        
        public int MaxConcurrentChats { get; set; } = 5;
        
        public Guid TenantId { get; set; }
        
        public Guid CreatedBy { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public enum AgentRole
    {
        Agent,
        SeniorAgent,
        TeamLead,
        Supervisor,
        Manager
    }

    public enum AgentStatus
    {
        Offline,
        Available,
        Busy,
        Away,
        DoNotDisturb
    }

    public class AgentResponse
    {
        public Guid Id { get; set; }
        
        public string Email { get; set; } = string.Empty;
        
        public string FirstName { get; set; } = string.Empty;
        
        public string LastName { get; set; } = string.Empty;
        
        public string FullName => $"{FirstName} {LastName}";
        
        public string? PhoneNumber { get; set; }
        
        public string? Department { get; set; }
        
        public string[] Skills { get; set; } = Array.Empty<string>();
        
        public string[] Languages { get; set; } = Array.Empty<string>();
        
        public AgentRole Role { get; set; }
        
        public AgentStatus Status { get; set; }
        
        public int MaxConcurrentChats { get; set; }
        
        public int CurrentActiveChats { get; set; }
        
        public bool IsActive { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? LastActiveAt { get; set; }
        
        public AgentPerformanceMetrics? Performance { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class AgentPerformanceMetrics
    {
        public int TotalChatsHandled { get; set; }
        
        public double AverageResponseTime { get; set; }
        
        public double AverageResolutionTime { get; set; }
        
        public double CustomerSatisfactionScore { get; set; }
        
        public int TicketsResolved { get; set; }
        
        public int TicketsEscalated { get; set; }
        
        public double ResolutionRate { get; set; }
        
        public TimeSpan TotalOnlineTime { get; set; }
        
        public DateTime PeriodStart { get; set; }
        
        public DateTime PeriodEnd { get; set; }
    }

    public class UpdateAgentRequest
    {
        public Guid AgentId { get; set; }
        
        public Guid TenantId { get; set; }
        
        public Guid UpdatedBy { get; set; }
        
        public string? FirstName { get; set; }
        
        public string? LastName { get; set; }
        
        public string? PhoneNumber { get; set; }
        
        public string? Department { get; set; }
        
        public string[]? Skills { get; set; }
        
        public string[]? Languages { get; set; }
        
        public AgentRole? Role { get; set; }
        
        public int? MaxConcurrentChats { get; set; }
        
        public bool? IsActive { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class UpdateAgentStatusRequest
    {
        public Guid AgentId { get; set; }
        
        public Guid TenantId { get; set; }
        
        public Guid UpdatedBy { get; set; }
        
        [Required]
        public string Status { get; set; } = string.Empty;
        
        public string? StatusMessage { get; set; }
        
        public DateTime? AvailableUntil { get; set; }
    }

    public class GetTicketsRequest
    {
        public Guid TenantId { get; set; }
        
        public string? Status { get; set; }
        
        public string? Priority { get; set; }
        
        public Guid? AssignedAgentId { get; set; }
        
        public Guid? CustomerId { get; set; }
        
        public string? Category { get; set; }
        
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        public int Page { get; set; } = 1;
        
        public int PageSize { get; set; } = 20;
    }

    public class GetTicketsResponse
    {
        public TicketResponse[] Tickets { get; set; } = Array.Empty<TicketResponse>();
        
        public int TotalCount { get; set; }
        
        public int Page { get; set; }
        
        public int PageSize { get; set; }
        
        public int TotalPages { get; set; }
    }

    public class CreateTicketRequest
    {
        [Required]
        public string Subject { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        public TicketPriority Priority { get; set; } = TicketPriority.Medium;
        
        public string? Category { get; set; }
        
        public Guid? CustomerId { get; set; }
        
        public string? CustomerEmail { get; set; }
        
        public string? CustomerName { get; set; }
        
        public Guid TenantId { get; set; }
        
        public Guid CreatedBy { get; set; }
        
        public Guid? AssignedAgentId { get; set; }
        
        public string? Source { get; set; }
        
        public Dictionary<string, object>? CustomFields { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public enum TicketPriority
    {
        Low,
        Medium,
        High,
        Critical,
        Urgent
    }

    public enum TicketStatus
    {
        New,
        Open,
        InProgress,
        Pending,
        Resolved,
        Closed,
        Escalated
    }

    public class TicketResponse
    {
        public Guid Id { get; set; }
        
        public string TicketNumber { get; set; } = string.Empty;
        
        public string Subject { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public TicketStatus Status { get; set; }
        
        public TicketPriority Priority { get; set; }
        
        public string? Category { get; set; }
        
        public Guid? CustomerId { get; set; }
        
        public string? CustomerEmail { get; set; }
        
        public string? CustomerName { get; set; }
        
        public Guid? AssignedAgentId { get; set; }
        
        public AgentResponse? AssignedAgent { get; set; }
        
        public string? Source { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
        
        public DateTime? ResolvedAt { get; set; }
        
        public TimeSpan? ResolutionTime { get; set; }
        
        public Dictionary<string, object>? CustomFields { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class AssignTicketRequest
    {
        public Guid TicketId { get; set; }
        
        public Guid TenantId { get; set; }
        
        [Required]
        public Guid AgentId { get; set; }
        
        public Guid AssignedBy { get; set; }
        
        public string? AssignmentReason { get; set; }
        
        public bool NotifyAgent { get; set; } = true;
    }

    public class EscalateTicketRequest
    {
        public Guid TicketId { get; set; }
        
        public Guid TenantId { get; set; }
        
        public Guid EscalatedBy { get; set; }
        
        [Required]
        public string Reason { get; set; } = string.Empty;
        
        public Guid? EscalatedToAgentId { get; set; }
        
        public string? EscalatedToDepartment { get; set; }
        
        public TicketPriority? NewPriority { get; set; }
        
        public string? Notes { get; set; }
    }

    public class EscalationResponse
    {
        public Guid Id { get; set; }
        
        public Guid TicketId { get; set; }
        
        public string Reason { get; set; } = string.Empty;
        
        public Guid EscalatedBy { get; set; }
        
        public AgentResponse? EscalatedByAgent { get; set; }
        
        public Guid? EscalatedToAgentId { get; set; }
        
        public AgentResponse? EscalatedToAgent { get; set; }
        
        public string? EscalatedToDepartment { get; set; }
        
        public TicketPriority? PreviousPriority { get; set; }
        
        public TicketPriority? NewPriority { get; set; }
        
        public string? Notes { get; set; }
        
        public DateTime EscalatedAt { get; set; }
        
        public DateTime? ResolvedAt { get; set; }
        
        public EscalationStatus Status { get; set; }
    }

    public enum EscalationStatus
    {
        Pending,
        Acknowledged,
        InProgress,
        Resolved,
        Rejected
    }

    public class GetAgentConversationsRequest
    {
        public Guid TenantId { get; set; }
        
        public Guid AgentId { get; set; }
        
        public string? Status { get; set; }
        
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        public int Page { get; set; } = 1;
        
        public int PageSize { get; set; } = 20;
    }

    public class GetAgentConversationsResponse
    {
        public AgentConversationResponse[] Conversations { get; set; } = Array.Empty<AgentConversationResponse>();
        
        public int TotalCount { get; set; }
        
        public int Page { get; set; }
        
        public int PageSize { get; set; }
        
        public int TotalPages { get; set; }
    }

    public class AgentConversationResponse
    {
        public Guid Id { get; set; }
        
        public string? CustomerName { get; set; }
        
        public string? CustomerEmail { get; set; }
        
        public ConversationStatus Status { get; set; }
        
        public DateTime StartedAt { get; set; }
        
        public DateTime? EndedAt { get; set; }
        
        public TimeSpan? Duration { get; set; }
        
        public int MessageCount { get; set; }
        
        public string? LastMessage { get; set; }
        
        public DateTime? LastMessageAt { get; set; }
        
        public string? Subject { get; set; }
        
        public ConversationPriority Priority { get; set; }
        
        public string[] Tags { get; set; } = Array.Empty<string>();
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public enum ConversationStatus
    {
        Active,
        Waiting,
        Resolved,
        Closed,
        Transferred
    }

    public enum ConversationPriority
    {
        Low,
        Normal,
        High,
        Urgent
    }

    public class JoinConversationRequest
    {
        public Guid ConversationId { get; set; }
        
        public Guid AgentId { get; set; }
        
        public Guid TenantId { get; set; }
        
        public string? JoinMessage { get; set; }
    }

    public class SendAgentMessageRequest
    {
        public Guid ConversationId { get; set; }
        
        public Guid AgentId { get; set; }
        
        public Guid TenantId { get; set; }
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        public AgentMessageType Type { get; set; } = AgentMessageType.Text;
        
        public string? AttachmentUrl { get; set; }
        
        public string? AttachmentType { get; set; }
        
        public bool IsInternal { get; set; } = false;
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public enum AgentMessageType
    {
        Text,
        Image,
        File,
        Audio,
        Video,
        SystemMessage,
        InternalNote
    }

    public class AgentMessageResponse
    {
        public Guid Id { get; set; }
        
        public Guid ConversationId { get; set; }
        
        public Guid AgentId { get; set; }
        
        public AgentResponse? Agent { get; set; }
        
        public string Message { get; set; } = string.Empty;
        
        public AgentMessageType Type { get; set; }
        
        public string? AttachmentUrl { get; set; }
        
        public string? AttachmentType { get; set; }
        
        public bool IsInternal { get; set; }
        
        public DateTime SentAt { get; set; }
        
        public DateTime? ReadAt { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class GetAgentPerformanceRequest
    {
        public Guid TenantId { get; set; }
        
        public Guid? AgentId { get; set; }
        
        public DateTime StartDate { get; set; }
        
        public DateTime EndDate { get; set; }
        
        public string[]? Metrics { get; set; }
        
        public string Granularity { get; set; } = "daily";
    }

    public class AgentPerformanceResponse
    {
        public AgentPerformanceData[] Performance { get; set; } = Array.Empty<AgentPerformanceData>();
        
        public AgentPerformanceSummary Summary { get; set; } = new();
        
        public DateTime PeriodStart { get; set; }
        
        public DateTime PeriodEnd { get; set; }
    }

    public class AgentPerformanceData
    {
        public Guid? AgentId { get; set; }
        
        public AgentResponse? Agent { get; set; }
        
        public DateTime Date { get; set; }
        
        public int ChatsHandled { get; set; }
        
        public double AverageResponseTime { get; set; }
        
        public double AverageResolutionTime { get; set; }
        
        public double CustomerSatisfactionScore { get; set; }
        
        public int TicketsResolved { get; set; }
        
        public int TicketsEscalated { get; set; }
        
        public double ResolutionRate { get; set; }
        
        public TimeSpan OnlineTime { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class AgentPerformanceSummary
    {
        public int TotalChatsHandled { get; set; }
        
        public double OverallAverageResponseTime { get; set; }
        
        public double OverallAverageResolutionTime { get; set; }
        
        public double OverallCustomerSatisfactionScore { get; set; }
        
        public int TotalTicketsResolved { get; set; }
        
        public int TotalTicketsEscalated { get; set; }
        
        public double OverallResolutionRate { get; set; }
        
        public TimeSpan TotalOnlineTime { get; set; }
        
        public Dictionary<string, int> PerformanceByAgent { get; set; } = new();
        
        public Dictionary<string, double> MetricTrends { get; set; } = new();
    }

    public class AgentHub
    {
        public string ConnectionId { get; set; } = string.Empty;
        
        public Guid AgentId { get; set; }
        
        public Guid TenantId { get; set; }
        
        public DateTime ConnectedAt { get; set; }
        
        public AgentStatus Status { get; set; }
        
        public Dictionary<string, object>? Context { get; set; }
    }

    public class AgentStatusUpdateEvent
    {
        public Guid AgentId { get; set; }
        
        public AgentStatus Status { get; set; }
        
        public string? StatusMessage { get; set; }
        
        public DateTime Timestamp { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class ConversationAssignmentEvent
    {
        public Guid ConversationId { get; set; }
        
        public Guid AgentId { get; set; }
        
        public string? CustomerName { get; set; }
        
        public ConversationPriority Priority { get; set; }
        
        public DateTime AssignedAt { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class TicketEscalationEvent
    {
        public Guid TicketId { get; set; }
        
        public string TicketNumber { get; set; } = string.Empty;
        
        public Guid? FromAgentId { get; set; }
        
        public Guid? ToAgentId { get; set; }
        
        public string? ToDepartment { get; set; }
        
        public string Reason { get; set; } = string.Empty;
        
        public TicketPriority Priority { get; set; }
        
        public DateTime EscalatedAt { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
