using Microsoft.Extensions.Logging;
using Arif.Platform.Shared.Common.Authentication;
using Arif.Platform.Shared.Common.Security;
using Arif.Platform.Authentication.Domain.Interfaces;
using Arif.Platform.Authentication.Domain.DTOs;

namespace Arif.Platform.Authentication.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditLogger _auditLogger;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        IJwtTokenService jwtTokenService,
        IPasswordHasher passwordHasher,
        IAuditLogger auditLogger,
        ILogger<AuthenticationService> logger)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
        _auditLogger = auditLogger;
        _logger = logger;
    }

    public async Task<AuthenticationResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Login attempt with non-existent email: {Email}", request.Email);
                await _auditLogger.LogSecurityEventAsync(SecurityEventType.LoginFailure, 
                    "Login attempt with non-existent email", null, null, 
                    new { Email = request.Email }, cancellationToken);
                return new AuthenticationResult(false, ErrorMessage: "Invalid email or password");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login attempt with inactive user: {UserId}", user.Id);
                return new AuthenticationResult(false, ErrorMessage: "Account is inactive");
            }

            if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Invalid password attempt for user: {UserId}", user.Id);
                await _auditLogger.LogSecurityEventAsync(SecurityEventType.LoginFailure, 
                    "Invalid password attempt", user.Id, user.TenantId, 
                    new { Email = request.Email }, cancellationToken);
                return new AuthenticationResult(false, ErrorMessage: "Invalid email or password");
            }

            if (!string.IsNullOrEmpty(request.TenantSubdomain))
            {
                var tenant = await _tenantRepository.GetBySubdomainAsync(request.TenantSubdomain, cancellationToken);
                if (tenant == null || tenant.Id != user.TenantId)
                {
                    _logger.LogWarning("Invalid tenant access attempt: {UserId}, {Subdomain}", user.Id, request.TenantSubdomain);
                    return new AuthenticationResult(false, ErrorMessage: "Invalid tenant access");
                }
            }

            var roles = await _userRepository.GetUserRolesAsync(user.Id, cancellationToken);
            var permissions = await _userRepository.GetUserPermissionsAsync(user.Id, cancellationToken);

            var accessToken = _jwtTokenService.GenerateAccessToken(
                user.Id, user.Email, roles, permissions, user.TenantId);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            user.LastLoginAt = DateTime.UtcNow;
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(30);
            
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            var userInfo = new UserInfo(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Username,
                user.IsActive,
                user.CreatedAt,
                user.LastLoginAt,
                user.Language,
                user.TimeZone,
                user.TenantId,
                user.Tenant?.Name ?? "",
                roles,
                permissions
            );

            await _auditLogger.LogSecurityEventAsync(SecurityEventType.LoginSuccess, 
                "Successful user login", user.Id, user.TenantId, 
                new { Email = request.Email, TenantSubdomain = request.TenantSubdomain }, cancellationToken);
            
            _logger.LogInformation("Successful login for user: {UserId}", user.Id);
            return new AuthenticationResult(
                true, 
                accessToken, 
                refreshToken, 
                DateTime.UtcNow.AddHours(1), 
                userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            return new AuthenticationResult(false, ErrorMessage: "An error occurred during login");
        }
    }

    public async Task<AuthenticationResult> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.FirstOrDefaultAsync(
                u => u.RefreshToken == request.RefreshToken && 
                     u.RefreshTokenExpiresAt > DateTime.UtcNow, 
                cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("Invalid refresh token attempt");
                return new AuthenticationResult(false, ErrorMessage: "Invalid refresh token");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Refresh token attempt with inactive user: {UserId}", user.Id);
                return new AuthenticationResult(false, ErrorMessage: "Account is inactive");
            }

            var roles = await _userRepository.GetUserRolesAsync(user.Id, cancellationToken);
            var permissions = await _userRepository.GetUserPermissionsAsync(user.Id, cancellationToken);

            var accessToken = _jwtTokenService.GenerateAccessToken(
                user.Id, user.Email, roles, permissions, user.TenantId);
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(30);
            
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            var userInfo = new UserInfo(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Username,
                user.IsActive,
                user.CreatedAt,
                user.LastLoginAt,
                user.Language,
                user.TimeZone,
                user.TenantId,
                user.Tenant?.Name ?? "",
                roles,
                permissions
            );

            _logger.LogInformation("Successful token refresh for user: {UserId}", user.Id);
            return new AuthenticationResult(
                true, 
                accessToken, 
                newRefreshToken, 
                DateTime.UtcNow.AddHours(1), 
                userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return new AuthenticationResult(false, ErrorMessage: "An error occurred during token refresh");
        }
    }

    public async Task<bool> LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null) return false;

            user.RefreshToken = null;
            user.RefreshTokenExpiresAt = null;
            
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            await _auditLogger.LogSecurityEventAsync(SecurityEventType.Logout, 
                "User logged out", userId, user.TenantId, null, cancellationToken);
            
            _logger.LogInformation("User logged out: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var principal = _jwtTokenService.ValidateToken(token);
            return principal != null && !_jwtTokenService.IsTokenExpired(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return false;
        }
    }

    public async Task<UserInfo?> GetUserInfoAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetWithRolesAsync(userId, cancellationToken);
            if (user == null) return null;

            var roles = await _userRepository.GetUserRolesAsync(user.Id, cancellationToken);
            var permissions = await _userRepository.GetUserPermissionsAsync(user.Id, cancellationToken);

            return new UserInfo(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Username,
                user.IsActive,
                user.CreatedAt,
                user.LastLoginAt,
                user.Language,
                user.TimeZone,
                user.TenantId,
                user.Tenant?.Name ?? "",
                roles,
                permissions
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info for: {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null) return false;

            if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                _logger.LogWarning("Invalid current password for user: {UserId}", request.UserId);
                return false;
            }

            user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            await _auditLogger.LogSecurityEventAsync(SecurityEventType.PasswordChange, 
                "User password changed", request.UserId, user.TenantId, null, cancellationToken);
            
            _logger.LogInformation("Password changed for user: {UserId}", request.UserId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", request.UserId);
            return false;
        }
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.FirstOrDefaultAsync(
                u => u.PasswordResetToken == request.Token && 
                     u.PasswordResetTokenExpiresAt > DateTime.UtcNow, 
                cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("Invalid password reset token");
                return false;
            }

            user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiresAt = null;
            user.UpdatedAt = DateTime.UtcNow;
            
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            await _auditLogger.LogSecurityEventAsync(SecurityEventType.PasswordReset, 
                "User password reset completed", user.Id, user.TenantId, null, cancellationToken);
            
            _logger.LogInformation("Password reset for user: {UserId}", user.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return false;
        }
    }

    public async Task<string> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null) return string.Empty;

            var resetToken = Guid.NewGuid().ToString();
            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);
            user.UpdatedAt = DateTime.UtcNow;
            
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Password reset token generated for user: {UserId}", user.Id);
            return resetToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating password reset token for email: {Email}", email);
            return string.Empty;
        }
    }

    public async Task<bool> ValidatePasswordResetTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.FirstOrDefaultAsync(
                u => u.PasswordResetToken == token && 
                     u.PasswordResetTokenExpiresAt > DateTime.UtcNow, 
                cancellationToken);

            return user != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating password reset token");
            return false;
        }
    }
}
