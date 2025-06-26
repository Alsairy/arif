using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "ArifPlatformSecretKeyForJWTTokenGeneration2024!@#$%";
var issuer = jwtSettings["Issuer"] ?? "https://arif-platform.com";
var audience = jwtSettings["Audience"] ?? "arif-platform-users";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
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



builder.Services.AddOcelot()
    .AddConsul();



builder.Services.AddHealthChecks();

builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseAuthentication();

app.MapHealthChecks("/health");

app.MapGet("/", () => new
{
    Service = "Arif Platform API Gateway",
    Version = "1.0.0",
    Environment = app.Environment.EnvironmentName,
    Timestamp = DateTime.UtcNow,
    Routes = new[]
    {
        "/api/auth/* -> Authentication Service",
        "/api/ai/* -> AI Orchestration Service",
        "/api/chatbot/* -> Chatbot Runtime Service",
        "/api/workflow/* -> Workflow Engine Service",
        "/api/integrations/* -> Integration Gateway Service",
        "/api/analytics/* -> Analytics Service",
        "/api/subscriptions/* -> Subscription Service",
        "/api/notifications/* -> Notification Service",
        "/api/live-agent/* -> Live Agent Service",
        "/api/config/* -> Configuration Management Service"
    }
});

await app.UseOcelot();

app.Run();
