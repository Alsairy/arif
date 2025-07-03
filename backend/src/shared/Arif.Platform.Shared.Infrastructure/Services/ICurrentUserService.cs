namespace Arif.Platform.Shared.Infrastructure.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserEmail { get; }
    Guid? TenantId { get; }
    bool IsAuthenticated { get; }
    IEnumerable<string> Roles { get; }
    IEnumerable<string> Permissions { get; }
}
