using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;
using Arif.Platform.Shared.Infrastructure.Data;
using Arif.Platform.Shared.Infrastructure.Services;
using Arif.Platform.Shared.Infrastructure.Middleware;
using Arif.Platform.Shared.Common.Authentication;
using Arif.Platform.Shared.Common.Security;
using Arif.Platform.Authentication.Domain.Interfaces;
using Arif.Platform.Authentication.Infrastructure.Repositories;
using Arif.Platform.Authentication.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Arif Platform Authentication API",
        Version = "v1",
        Description = "Authentication and authorization service for the Arif Platform with multi-tenant support and Arabic language capabilities.",
        Contact = new OpenApiContact
        {
            Name = "Arif Platform Team",
            Email = "support@arif-platform.com"
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<ArifPlatformDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
        options.UseInMemoryDatabase("ArifPlatformInMemory");
    }
    else
    {
        options.UseSqlite(connectionString);
    }
});

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "ArifPlatformSecretKeyForJWTTokenGeneration2024!@#$%";
var issuer = jwtSettings["Issuer"] ?? "https://arif-platform.com";
var audience = jwtSettings["Audience"] ?? "arif-platform-users";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ArifCorsPolicy", policy =>
    {
        policy.WithOrigins(
                "https://arif-code-review-app-xczsu670.devinapps.com",
                "https://arif-code-review-app-ddxl77iy.devinapps.com",
                "https://arif-code-review-app-ec5kdlfl.devinapps.com",
                "https://arif-code-review-app-gzmr1uth.devinapps.com",
                "https://arif-codebase-checker-a0sau4n1.devinapps.com",
                "https://arif-code-review-app-tunnel-9llvv5qw.devinapps.com",
                "https://user:a4d5aa1b961b77df777c8d640c833529@arif-code-review-app-tunnel-9llvv5qw.devinapps.com"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<ITenantManagementService, TenantManagementService>();

builder.Services.AddScoped<IAuditLogger, AuditLogger>();
builder.Services.AddScoped<IDataEncryption, DataEncryption>();
builder.Services.AddScoped<IFieldEncryption>(provider => provider.GetService<IDataEncryption>() as IFieldEncryption);
builder.Services.AddScoped<IGdprService, GdprService>();
builder.Services.AddScoped<IRateLimitingService, RateLimitingService>();
builder.Services.AddScoped<IInputValidationService, InputValidationService>();
builder.Services.AddScoped<ISecurityMonitoringService, SecurityMonitoringService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo("/tmp/keys"))
        .SetApplicationName("Arif.Platform")
        .SetDefaultKeyLifetime(TimeSpan.FromDays(90));
}
else
{
    builder.Services.AddDataProtection()
        .PersistKeysToDbContext<ArifPlatformDbContext>()
        .SetApplicationName("Arif.Platform")
        .SetDefaultKeyLifetime(TimeSpan.FromDays(90));
}

builder.Services.Configure<RateLimitOptions>(options =>
{
    options.MaxRequests = 100;
    options.TimeWindow = TimeSpan.FromMinutes(1);
    options.KeyPrefix = "auth_rate_limit";
});

builder.Services.AddMemoryCache();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ArifPlatformDbContext>();

builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Arif Platform Authentication API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseCors("ArifCorsPolicy");

app.UseMiddleware<TenantResolutionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.MapGet("/", () => new
{
    Service = "Arif Platform Authentication Service",
    Version = "1.0.0",
    Environment = app.Environment.EnvironmentName,
    Timestamp = DateTime.UtcNow,
    Features = new[]
    {
        "JWT Authentication",
        "Multi-tenant Support",
        "Role-based Access Control",
        "Arabic Language Support",
        "User Management",
        "Tenant Management"
    }
});

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ArifPlatformDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        context.Database.EnsureCreated();
        
        if (!context.Users.Any(u => u.Email == "admin@example.com"))
        {
            var defaultTenant = context.Tenants.FirstOrDefault() ?? new Arif.Platform.Shared.Domain.Entities.Tenant
            {
                Id = Guid.NewGuid(),
                Name = "Default Admin Tenant",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            if (context.Tenants.FirstOrDefault() == null)
            {
                context.Tenants.Add(defaultTenant);
                context.SaveChanges();
            }

            var adminUser = new Arif.Platform.Shared.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Email = "admin@example.com",
                Username = "admin",
                FirstName = "Admin",
                LastName = "User",
                PasswordHash = passwordHasher.HashPassword("password"),
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                TenantId = defaultTenant.Id
            };
            
            context.Users.Add(adminUser);
            context.SaveChanges();
            logger.LogInformation("Admin user created successfully with email: admin@example.com and TenantId: {TenantId}", defaultTenant.Id);
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Could not ensure database creation or seeding. This is expected in production environments.");
    }
}

app.Run();
