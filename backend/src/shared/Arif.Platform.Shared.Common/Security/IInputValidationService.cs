namespace Arif.Platform.Shared.Common.Security;

public interface IInputValidationService
{
    ValidationResult ValidateEmail(string email);
    ValidationResult ValidatePassword(string password);
    ValidationResult ValidatePhoneNumber(string phoneNumber);
    ValidationResult ValidateUrl(string url);
    ValidationResult ValidateJson(string json);
    ValidationResult ValidateXml(string xml);
    ValidationResult ValidateHtml(string html);
    ValidationResult ValidateSqlInput(string input);
    string SanitizeInput(string input);
    string SanitizeHtml(string html);
    bool IsValidArabicText(string text);
    bool ContainsSuspiciousPatterns(string input);
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? SanitizedValue { get; set; }
}

public enum ValidationType
{
    Email,
    Password,
    PhoneNumber,
    Url,
    Json,
    Xml,
    Html,
    SqlInput,
    ArabicText,
    GeneralText
}
