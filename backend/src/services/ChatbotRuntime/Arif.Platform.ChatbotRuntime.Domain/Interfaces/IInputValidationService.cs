using Arif.Platform.ChatbotRuntime.Domain.DTOs;

namespace Arif.Platform.ChatbotRuntime.Domain.Interfaces;

public interface IInputValidationService
{
    Task<ValidationResult> ValidateUserInputAsync(string input, string sessionId);
    Task<ValidationResult> ValidateFileUploadAsync(Stream fileStream, string fileName, string sessionId);
    Task<bool> IsInputSafeAsync(string input);
    Task<string> SanitizeInputAsync(string input);
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> Warnings { get; set; } = new();
}
