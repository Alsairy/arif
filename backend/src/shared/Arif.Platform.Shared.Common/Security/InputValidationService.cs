using System.Text.RegularExpressions;
using System.Text.Json;
using System.Xml;
using Microsoft.Extensions.Logging;
using HtmlAgilityPack;

namespace Arif.Platform.Shared.Common.Security;

public class InputValidationService : IInputValidationService
{
    private readonly ILogger<InputValidationService> _logger;
    
    private static readonly Regex EmailRegex = new(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled);
    private static readonly Regex PhoneRegex = new(@"^\+?[1-9]\d{1,14}$", RegexOptions.Compiled);
    private static readonly Regex UrlRegex = new(@"^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)$", RegexOptions.Compiled);
    private static readonly Regex ArabicTextRegex = new(@"[\u0600-\u06FF\u0750-\u077F\u08A0-\u08FF\uFB50-\uFDFF\uFE70-\uFEFF]", RegexOptions.Compiled);
    
    private static readonly string[] SuspiciousPatterns = {
        "<script", "javascript:", "vbscript:", "onload=", "onerror=", "onclick=",
        "SELECT", "INSERT", "UPDATE", "DELETE", "DROP", "UNION", "EXEC",
        "../", "..\\", "/etc/passwd", "cmd.exe", "powershell",
        "eval(", "setTimeout(", "setInterval("
    };

    private static readonly string[] SqlKeywords = {
        "SELECT", "INSERT", "UPDATE", "DELETE", "DROP", "CREATE", "ALTER",
        "UNION", "EXEC", "EXECUTE", "DECLARE", "CAST", "CONVERT"
    };

    public InputValidationService(ILogger<InputValidationService> logger)
    {
        _logger = logger;
    }

    public ValidationResult ValidateEmail(string email)
    {
        var result = new ValidationResult();
        
        if (string.IsNullOrWhiteSpace(email))
        {
            result.Errors.Add("Email is required");
            return result;
        }

        if (email.Length > 254)
        {
            result.Errors.Add("Email is too long");
            return result;
        }

        if (!EmailRegex.IsMatch(email))
        {
            result.Errors.Add("Invalid email format");
            return result;
        }

        if (ContainsSuspiciousPatterns(email))
        {
            result.Errors.Add("Email contains suspicious patterns");
            _logger.LogWarning("Suspicious email pattern detected: {Email}", email);
            return result;
        }

        result.IsValid = true;
        result.SanitizedValue = email.Trim().ToLowerInvariant();
        return result;
    }

    public ValidationResult ValidatePassword(string password)
    {
        var result = new ValidationResult();
        
        if (string.IsNullOrWhiteSpace(password))
        {
            result.Errors.Add("Password is required");
            return result;
        }

        if (password.Length < 8)
        {
            result.Errors.Add("Password must be at least 8 characters long");
        }

        if (password.Length > 128)
        {
            result.Errors.Add("Password is too long");
        }

        if (!password.Any(char.IsUpper))
        {
            result.Errors.Add("Password must contain at least one uppercase letter");
        }

        if (!password.Any(char.IsLower))
        {
            result.Errors.Add("Password must contain at least one lowercase letter");
        }

        if (!password.Any(char.IsDigit))
        {
            result.Errors.Add("Password must contain at least one digit");
        }

        if (!password.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c)))
        {
            result.Errors.Add("Password must contain at least one special character");
        }

        result.IsValid = result.Errors.Count == 0;
        return result;
    }

    public ValidationResult ValidatePhoneNumber(string phoneNumber)
    {
        var result = new ValidationResult();
        
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            result.Errors.Add("Phone number is required");
            return result;
        }

        var cleanPhone = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
        
        if (!PhoneRegex.IsMatch(cleanPhone))
        {
            result.Errors.Add("Invalid phone number format");
            return result;
        }

        result.IsValid = true;
        result.SanitizedValue = cleanPhone;
        return result;
    }

    public ValidationResult ValidateUrl(string url)
    {
        var result = new ValidationResult();
        
        if (string.IsNullOrWhiteSpace(url))
        {
            result.Errors.Add("URL is required");
            return result;
        }

        if (!UrlRegex.IsMatch(url))
        {
            result.Errors.Add("Invalid URL format");
            return result;
        }

        if (ContainsSuspiciousPatterns(url))
        {
            result.Errors.Add("URL contains suspicious patterns");
            _logger.LogWarning("Suspicious URL pattern detected: {Url}", url);
            return result;
        }

        result.IsValid = true;
        result.SanitizedValue = url.Trim();
        return result;
    }

    public ValidationResult ValidateJson(string json)
    {
        var result = new ValidationResult();
        
        if (string.IsNullOrWhiteSpace(json))
        {
            result.Errors.Add("JSON is required");
            return result;
        }

        try
        {
            JsonDocument.Parse(json);
            result.IsValid = true;
            result.SanitizedValue = json.Trim();
        }
        catch (JsonException ex)
        {
            result.Errors.Add($"Invalid JSON format: {ex.Message}");
        }

        return result;
    }

    public ValidationResult ValidateXml(string xml)
    {
        var result = new ValidationResult();
        
        if (string.IsNullOrWhiteSpace(xml))
        {
            result.Errors.Add("XML is required");
            return result;
        }

        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            result.IsValid = true;
            result.SanitizedValue = xml.Trim();
        }
        catch (XmlException ex)
        {
            result.Errors.Add($"Invalid XML format: {ex.Message}");
        }

        return result;
    }

    public ValidationResult ValidateHtml(string html)
    {
        var result = new ValidationResult();
        
        if (string.IsNullOrWhiteSpace(html))
        {
            result.IsValid = true;
            result.SanitizedValue = string.Empty;
            return result;
        }

        if (ContainsSuspiciousPatterns(html))
        {
            result.Errors.Add("HTML contains suspicious patterns");
            _logger.LogWarning("Suspicious HTML pattern detected");
            return result;
        }

        try
        {
            var sanitized = SanitizeHtml(html);
            result.IsValid = true;
            result.SanitizedValue = sanitized;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"HTML validation failed: {ex.Message}");
        }

        return result;
    }

    public ValidationResult ValidateSqlInput(string input)
    {
        var result = new ValidationResult();
        
        if (string.IsNullOrWhiteSpace(input))
        {
            result.IsValid = true;
            result.SanitizedValue = string.Empty;
            return result;
        }

        var upperInput = input.ToUpperInvariant();
        foreach (var keyword in SqlKeywords)
        {
            if (upperInput.Contains(keyword))
            {
                result.Errors.Add("Input contains SQL keywords");
                _logger.LogWarning("SQL injection attempt detected: {Input}", input);
                return result;
            }
        }

        if (input.Contains("'") || input.Contains("\"") || input.Contains(";") || input.Contains("--"))
        {
            result.Errors.Add("Input contains potentially dangerous SQL characters");
            _logger.LogWarning("Suspicious SQL characters detected: {Input}", input);
            return result;
        }

        result.IsValid = true;
        result.SanitizedValue = SanitizeInput(input);
        return result;
    }

    public string SanitizeInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return input
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#x27;")
            .Replace("/", "&#x2F;")
            .Trim();
    }

    public string SanitizeHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var allowedTags = new[] { "p", "br", "strong", "em", "u", "ol", "ul", "li", "h1", "h2", "h3", "h4", "h5", "h6" };
        var nodesToRemove = new List<HtmlNode>();

        foreach (var node in doc.DocumentNode.Descendants())
        {
            if (node.NodeType == HtmlNodeType.Element)
            {
                if (!allowedTags.Contains(node.Name.ToLowerInvariant()))
                {
                    nodesToRemove.Add(node);
                }
                else
                {
                    node.Attributes.RemoveAll();
                }
            }
        }

        foreach (var node in nodesToRemove)
        {
            node.Remove();
        }

        return doc.DocumentNode.InnerHtml;
    }

    public bool IsValidArabicText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return true;

        return ArabicTextRegex.IsMatch(text);
    }

    public bool ContainsSuspiciousPatterns(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var lowerInput = input.ToLowerInvariant();
        return SuspiciousPatterns.Any(pattern => lowerInput.Contains(pattern.ToLowerInvariant()));
    }
}
