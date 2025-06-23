namespace Arif.Platform.Authentication.Domain.DTOs;

public record LoginRequest(
    string Email,
    string Password,
    string? TenantSubdomain = null,
    bool RememberMe = false
);

public record RefreshTokenRequest(
    string RefreshToken
);

public record ChangePasswordRequest(
    Guid UserId,
    string CurrentPassword,
    string NewPassword
);

public record ResetPasswordRequest(
    string Token,
    string NewPassword
);

public record AuthenticationResult(
    bool Success,
    string? AccessToken = null,
    string? RefreshToken = null,
    DateTime? ExpiresAt = null,
    UserInfo? User = null,
    string? ErrorMessage = null
);

public record UserInfo(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? Username,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    string Language,
    string TimeZone,
    Guid TenantId,
    string TenantName,
    IEnumerable<string> Roles,
    IEnumerable<string> Permissions
);
