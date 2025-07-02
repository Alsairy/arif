using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.FileProviders;
using System.Text;
using Arif.Platform.Shared.Infrastructure.Data;
using Arif.Platform.Shared.Infrastructure.Services;
using Arif.Platform.Shared.Infrastructure.Middleware;
using Arif.Platform.Shared.Common.Authentication;
using Arif.Platform.ChatbotRuntime.Domain.Interfaces;
using Arif.Platform.ChatbotRuntime.Infrastructure.Services;
using Arif.Platform.ChatbotRuntime.Infrastructure.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Arif Platform Chatbot Runtime API",
        Version = "v1",
        Description = "Real-time chatbot execution engine with conversation management, session handling, and multi-channel support.",
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
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Server=(localdb)\\mssqllocaldb;Database=ArifPlatform;Trusted_Connection=true;MultipleActiveResultSets=true";
    options.UseSqlServer(connectionString);
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
                "https://arif-code-review-app-xczsu670.devinapps.com", // Admin Dashboard
                "https://arif-code-review-app-ddxl77iy.devinapps.com", // Tenant Dashboard
                "https://arif-code-review-app-ec5kdlfl.devinapps.com", // Agent Interface
                "https://arif-code-review-app-gzmr1uth.devinapps.com", // Chat Widget
                "https://arif-codebase-checker-a0sau4n1.devinapps.com"  // Landing Page
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddSignalR();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

builder.Services.AddScoped<IChatbotRuntimeService, ChatbotRuntimeService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IChannelService, ChannelService>();

builder.Services.AddHealthChecks();

builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Arif Platform Chatbot Runtime API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
    RequestPath = ""
});

app.UseCors("ArifCorsPolicy");

app.UseMiddleware<TenantResolutionMiddleware>();
app.UseMiddleware<ZeroTrustMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapHub<ChatHub>("/chatHub");

app.MapGet("/", () => new
{
    Service = "Arif Platform Chatbot Runtime Service",
    Version = "1.0.0",
    Environment = app.Environment.EnvironmentName,
    Timestamp = DateTime.UtcNow,
    Features = new[]
    {
        "Real-time Chat Processing",
        "Conversation Management",
        "Session Handling",
        "Multi-channel Support",
        "SignalR Real-time Communication",
        "Zero-Trust Security",
        "Arabic Language Support",
        "Bot Execution Engine"
    }
});

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ArifPlatformDbContext>();
    try
    {
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Could not ensure database creation. This is expected in production environments.");
    }
}

app.Run();
