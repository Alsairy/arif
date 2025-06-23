namespace Arif.Platform.Authentication.Domain.DTOs;

public record CreateUserRequest(
    string Email,
    string FirstName,
    string LastName,
    string? Username,
    string Password,
    Guid TenantId,
    string Language = "ar",
    string TimeZone = "Asia/Riyadh",
    IEnumerable<string>? Roles = null
);

public record UpdateUserRequest(
    Guid UserId,
    string? Email = null,
    string? FirstName = null,
    string? LastName = null,
    string? Username = null,
    string? Language = null,
    string? TimeZone = null,
    bool? IsActive = null
);

public record GetUsersRequest(
    Guid? TenantId = null,
    string? SearchTerm = null,
    string? Role = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20,
    string SortBy = "CreatedAt",
    bool SortDescending = true
);

public record AssignRoleRequest(
    Guid UserId,
    string RoleName
);

public record RemoveRoleRequest(
    Guid UserId,
    string RoleName
);

public record UserResult(
    bool Success,
    UserInfo? User = null,
    string? ErrorMessage = null
);
