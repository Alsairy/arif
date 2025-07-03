using System.ComponentModel.DataAnnotations;

namespace Arif.Platform.AIOrchestration.Domain.DTOs
{
    public class ChatRequest
    {
        [Required]
        public string Message { get; set; } = string.Empty;
        
        public string? ConversationId { get; set; }
        
        public string? Language { get; set; } = "ar";
        
        public Dictionary<string, object>? Context { get; set; }
        
        public string? ModelId { get; set; }
        
        public ChatSettings? Settings { get; set; }
    }

    public class ChatResponse
    {
        public string Response { get; set; } = string.Empty;
        
        public string ConversationId { get; set; } = string.Empty;
        
        public string Language { get; set; } = string.Empty;
        
        public double Confidence { get; set; }
        
        public string ModelUsed { get; set; } = string.Empty;
        
        public Dictionary<string, object>? Metadata { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class ChatSettings
    {
        public double Temperature { get; set; } = 0.7;
        
        public int MaxTokens { get; set; } = 1000;
        
        public bool UseRAG { get; set; } = true;
        
        public string[]? StopSequences { get; set; }
        
        public double TopP { get; set; } = 0.9;
    }

    public class RAGRequest
    {
        [Required]
        public string Query { get; set; } = string.Empty;
        
        public string? Language { get; set; } = "ar";
        
        public string[]? DocumentIds { get; set; }
        
        public int MaxResults { get; set; } = 5;
        
        public double SimilarityThreshold { get; set; } = 0.7;
        
        public Dictionary<string, object>? Filters { get; set; }
    }

    public class RAGResponse
    {
        public string Answer { get; set; } = string.Empty;
        
        public RAGSource[] Sources { get; set; } = Array.Empty<RAGSource>();
        
        public double Confidence { get; set; }
        
        public string Language { get; set; } = string.Empty;
        
        public Dictionary<string, object>? Metadata { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class RAGSource
    {
        public string DocumentId { get; set; } = string.Empty;
        
        public string Title { get; set; } = string.Empty;
        
        public string Content { get; set; } = string.Empty;
        
        public double Similarity { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class AIModel
    {
        public string Id { get; set; } = string.Empty;
        
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public string Provider { get; set; } = string.Empty;
        
        public string[] Languages { get; set; } = Array.Empty<string>();
        
        public string[] Capabilities { get; set; } = Array.Empty<string>();
        
        public bool IsAvailable { get; set; }
        
        public Dictionary<string, object>? Configuration { get; set; }
    }

    public class FineTuneRequest
    {
        [Required]
        public string ModelId { get; set; } = string.Empty;
        
        [Required]
        public string TrainingDataUrl { get; set; } = string.Empty;
        
        public string? ValidationDataUrl { get; set; }
        
        public string JobName { get; set; } = string.Empty;
        
        public Dictionary<string, object>? Hyperparameters { get; set; }
        
        public string? Description { get; set; }
    }

    public class FineTuneResponse
    {
        public string JobId { get; set; } = string.Empty;
        
        public string Status { get; set; } = string.Empty;
        
        public string ModelId { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class ConversationContext
    {
        public string ConversationId { get; set; } = string.Empty;
        
        public ConversationMessage[] Messages { get; set; } = Array.Empty<ConversationMessage>();
        
        public Dictionary<string, object>? UserContext { get; set; }
        
        public string Language { get; set; } = string.Empty;
        
        public DateTime LastActivity { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class ConversationMessage
    {
        public string Role { get; set; } = string.Empty;
        
        public string Content { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class EmbeddingRequest
    {
        [Required]
        public IEnumerable<string> Texts { get; set; } = Enumerable.Empty<string>();
        
        public string? ModelId { get; set; }
        
        public string? Language { get; set; } = "ar";
        
        public Dictionary<string, object>? Options { get; set; }
    }

    public class EmbeddingResponse
    {
        public EmbeddingResult[] Results { get; set; } = Array.Empty<EmbeddingResult>();
        
        public string ModelUsed { get; set; } = string.Empty;
        
        public Dictionary<string, object>? Metadata { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class EmbeddingResult
    {
        public string Text { get; set; } = string.Empty;
        
        public float[] Embedding { get; set; } = Array.Empty<float>();
        
        public int TokenCount { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
