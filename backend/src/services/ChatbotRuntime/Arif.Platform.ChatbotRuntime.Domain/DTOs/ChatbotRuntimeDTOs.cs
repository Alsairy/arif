using System.ComponentModel.DataAnnotations;

namespace Arif.Platform.ChatbotRuntime.Domain.DTOs
{
    public class ChatMessageRequest
    {
        [Required]
        public string Message { get; set; } = string.Empty;
        
        public string SessionId { get; set; } = string.Empty;
        
        public string ChannelId { get; set; } = string.Empty;
        
        public string? UserId { get; set; }
        
        public string? UserName { get; set; }
        
        public Dictionary<string, object>? Context { get; set; }
        
        public string Language { get; set; } = "ar";
        
        public MessageType Type { get; set; } = MessageType.Text;
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class ChatMessageResponse
    {
        public string Message { get; set; } = string.Empty;
        
        public string SessionId { get; set; } = string.Empty;
        
        public string ConversationId { get; set; } = string.Empty;
        
        public MessageType Type { get; set; } = MessageType.Text;
        
        public ChatAction[] Actions { get; set; } = Array.Empty<ChatAction>();
        
        public ChatSuggestion[] Suggestions { get; set; } = Array.Empty<ChatSuggestion>();
        
        public Dictionary<string, object>? Context { get; set; }
        
        public bool IsTyping { get; set; }
        
        public int TypingDuration { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
        
        public DateTime Timestamp { get; set; }
    }

    public class ChatAction
    {
        public string Type { get; set; } = string.Empty;
        
        public string Label { get; set; } = string.Empty;
        
        public string Value { get; set; } = string.Empty;
        
        public Dictionary<string, object>? Parameters { get; set; }
    }

    public class ChatSuggestion
    {
        public string Text { get; set; } = string.Empty;
        
        public string Value { get; set; } = string.Empty;
        
        public string? Icon { get; set; }
    }

    public enum MessageType
    {
        Text,
        Image,
        File,
        Audio,
        Video,
        Location,
        Contact,
        QuickReply,
        Card,
        Carousel,
        List,
        Button
    }

    public class CreateSessionRequest
    {
        [Required]
        public string ChannelId { get; set; } = string.Empty;
        
        public string? UserId { get; set; }
        
        public string? UserName { get; set; }
        
        public Dictionary<string, object>? InitialContext { get; set; }
        
        public string Language { get; set; } = "ar";
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class SessionResponse
    {
        public Guid Id { get; set; }
        
        public string ChannelId { get; set; } = string.Empty;
        
        public string? UserId { get; set; }
        
        public string? UserName { get; set; }
        
        public string Status { get; set; } = string.Empty;
        
        public DateTime StartedAt { get; set; }
        
        public DateTime? EndedAt { get; set; }
        
        public Dictionary<string, object>? Context { get; set; }
        
        public string Language { get; set; } = string.Empty;
        
        public Dictionary<string, object>? Metadata { get; set; }
        
        public int MessageCount { get; set; }
        
        public TimeSpan Duration { get; set; }
    }

    public class GetConversationsRequest
    {
        public Guid TenantId { get; set; }
        
        public string? SessionId { get; set; }
        
        public string? UserId { get; set; }
        
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        public string? Status { get; set; }
        
        public int Page { get; set; } = 1;
        
        public int PageSize { get; set; } = 20;
    }

    public class GetConversationsResponse
    {
        public ConversationSummary[] Items { get; set; } = Array.Empty<ConversationSummary>();
        
        public int TotalCount { get; set; }
        
        public int Page { get; set; }
        
        public int PageSize { get; set; }
        
        public int TotalPages { get; set; }
    }

    public class ConversationSummary
    {
        public Guid Id { get; set; }
        
        public string SessionId { get; set; } = string.Empty;
        
        public string ChannelId { get; set; } = string.Empty;
        
        public string? UserId { get; set; }
        
        public string? UserName { get; set; }
        
        public string Status { get; set; } = string.Empty;
        
        public DateTime StartedAt { get; set; }
        
        public DateTime? EndedAt { get; set; }
        
        public int MessageCount { get; set; }
        
        public string LastMessage { get; set; } = string.Empty;
        
        public DateTime LastMessageAt { get; set; }
        
        public string Language { get; set; } = string.Empty;
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class ConversationDetail
    {
        public Guid Id { get; set; }
        
        public string SessionId { get; set; } = string.Empty;
        
        public string ChannelId { get; set; } = string.Empty;
        
        public string? UserId { get; set; }
        
        public string? UserName { get; set; }
        
        public string Status { get; set; } = string.Empty;
        
        public DateTime StartedAt { get; set; }
        
        public DateTime? EndedAt { get; set; }
        
        public ChatMessage[] Messages { get; set; } = Array.Empty<ChatMessage>();
        
        public Dictionary<string, object>? Context { get; set; }
        
        public string Language { get; set; } = string.Empty;
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class GetConversationMessagesRequest
    {
        public Guid ConversationId { get; set; }
        
        public Guid TenantId { get; set; }
        
        public int Page { get; set; } = 1;
        
        public int PageSize { get; set; } = 50;
        
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
    }

    public class GetConversationMessagesResponse
    {
        public ChatMessage[] Items { get; set; } = Array.Empty<ChatMessage>();
        
        public int TotalCount { get; set; }
        
        public int Page { get; set; }
        
        public int PageSize { get; set; }
        
        public int TotalPages { get; set; }
    }

    public class ChatMessage
    {
        public Guid Id { get; set; }
        
        public Guid ConversationId { get; set; }
        
        public string Content { get; set; } = string.Empty;
        
        public MessageType Type { get; set; }
        
        public string Sender { get; set; } = string.Empty;
        
        public string SenderType { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; }
        
        public bool IsRead { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
        
        public ChatAttachment[] Attachments { get; set; } = Array.Empty<ChatAttachment>();
    }

    public class ChatAttachment
    {
        public string Type { get; set; } = string.Empty;
        
        public string Url { get; set; } = string.Empty;
        
        public string? Name { get; set; }
        
        public long? Size { get; set; }
        
        public string? MimeType { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class CreateChannelRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public string Type { get; set; } = "web";
        
        public Guid TenantId { get; set; }
        
        public Guid CreatedBy { get; set; }
        
        public ChannelConfiguration Configuration { get; set; } = new();
        
        public bool IsActive { get; set; } = true;
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class ChannelConfiguration
    {
        public string? WelcomeMessage { get; set; }
        
        public string[] AllowedMessageTypes { get; set; } = Array.Empty<string>();
        
        public int MaxMessageLength { get; set; } = 1000;
        
        public bool EnableTypingIndicator { get; set; } = true;
        
        public bool EnableReadReceipts { get; set; } = true;
        
        public string Theme { get; set; } = "default";
        
        public Dictionary<string, object>? CustomSettings { get; set; }
    }

    public class ChannelResponse
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public string Type { get; set; } = string.Empty;
        
        public Guid TenantId { get; set; }
        
        public string Status { get; set; } = string.Empty;
        
        public ChannelConfiguration Configuration { get; set; } = new();
        
        public bool IsActive { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
        
        public ChannelStatistics Statistics { get; set; } = new();
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class ChannelStatistics
    {
        public int TotalSessions { get; set; }
        
        public int ActiveSessions { get; set; }
        
        public int TotalMessages { get; set; }
        
        public int TodayMessages { get; set; }
        
        public double AverageSessionDuration { get; set; }
        
        public DateTime LastActivity { get; set; }
    }

    public class UpdateChannelRequest
    {
        public Guid ChannelId { get; set; }
        
        public Guid TenantId { get; set; }
        
        public Guid UpdatedBy { get; set; }
        
        public string? Name { get; set; }
        
        public string? Description { get; set; }
        
        public ChannelConfiguration? Configuration { get; set; }
        
        public bool? IsActive { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class ChatbotConfiguration
    {
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public string Language { get; set; } = "ar";
        
        public string[] SupportedLanguages { get; set; } = Array.Empty<string>();
        
        public string Personality { get; set; } = string.Empty;
        
        public ChatbotCapabilities Capabilities { get; set; } = new();
        
        public Dictionary<string, object>? CustomSettings { get; set; }
    }

    public class ChatbotCapabilities
    {
        public bool SupportsText { get; set; } = true;
        
        public bool SupportsImages { get; set; } = false;
        
        public bool SupportsFiles { get; set; } = false;
        
        public bool SupportsAudio { get; set; } = false;
        
        public bool SupportsVideo { get; set; } = false;
        
        public bool SupportsLocation { get; set; } = false;
        
        public bool SupportsQuickReplies { get; set; } = true;
        
        public bool SupportsCards { get; set; } = true;
        
        public bool SupportsCarousels { get; set; } = true;
        
        public bool SupportsLists { get; set; } = true;
        
        public bool SupportsButtons { get; set; } = true;
    }

    public class ChatHub
    {
        public string ConnectionId { get; set; } = string.Empty;
        
        public string? UserId { get; set; }
        
        public string SessionId { get; set; } = string.Empty;
        
        public string ChannelId { get; set; } = string.Empty;
        
        public DateTime ConnectedAt { get; set; }
        
        public Dictionary<string, object>? Context { get; set; }
    }

    public class RealTimeChatEvent
    {
        public string EventType { get; set; } = string.Empty;
        
        public string SessionId { get; set; } = string.Empty;
        
        public string? UserId { get; set; }
        
        public Dictionary<string, object>? Data { get; set; }
        
        public DateTime Timestamp { get; set; }
    }

    public class TypingIndicator
    {
        public string SessionId { get; set; } = string.Empty;
        
        public string? UserId { get; set; }
        
        public bool IsTyping { get; set; }
        
        public DateTime Timestamp { get; set; }
    }

    public class MessageDeliveryStatus
    {
        public Guid MessageId { get; set; }
        
        public string Status { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; }
        
        public string? ErrorMessage { get; set; }
    }
}
