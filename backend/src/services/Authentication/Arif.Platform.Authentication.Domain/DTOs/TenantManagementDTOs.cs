namespace Arif.Platform.Authentication.Domain.DTOs;

public record CreateTenantRequest(
    string Name,
    string Subdomain,
    string ContactEmail,
    string? Description = null,
    string Language = "ar",
    string TimeZone = "Asia/Riyadh",
    int MaxUsers = 100
);

public record UpdateTenantRequest(
    Guid TenantId,
    string? Name = null,
    string? Subdomain = null,
    string? ContactEmail = null,
    string? Description = null,
    string? Language = null,
    string? TimeZone = null,
    int? MaxUsers = null,
    bool? IsActive = null
);

public record GetTenantsRequest(
    string? SearchTerm = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20,
    string SortBy = "CreatedAt",
    bool SortDescending = true
);

public record TenantInfo(
    Guid Id,
    string Name,
    string Subdomain,
    string ContactEmail,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    string Language,
    string TimeZone,
    int MaxUsers,
    int CurrentUserCount
);

public record TenantResult(
    bool Success,
    TenantInfo? Tenant = null,
    string? ErrorMessage = null
);

public record TenantStats(
    Guid TenantId,
    string TenantName,
    int TotalUsers,
    int ActiveUsers,
    int InactiveUsers,
    int TotalChatbots,
    int ActiveChatbots,
    int TotalSessions,
    int TotalMessages,
    DateTime LastActivity
);
