using Microsoft.EntityFrameworkCore;
using Arif.Platform.ConfigurationManagement.Domain.Entities;

namespace Arif.Platform.ConfigurationManagement.Infrastructure.Data;

public class ConfigurationDbContext : DbContext
{
    public ConfigurationDbContext(DbContextOptions<ConfigurationDbContext> options) : base(options)
    {
    }

    public DbSet<Configuration> Configurations { get; set; }
    public DbSet<ConfigurationValidationRule> ConfigurationValidationRules { get; set; }
    public DbSet<FeatureFlag> FeatureFlags { get; set; }
    public DbSet<FeatureFlagRule> FeatureFlagRules { get; set; }
    public DbSet<FeatureFlagSchedule> FeatureFlagSchedules { get; set; }
    public DbSet<ConfigurationDeployment> ConfigurationDeployments { get; set; }
    public DbSet<ConfigurationDeploymentItem> ConfigurationDeploymentItems { get; set; }
    public DbSet<ConfigurationAuditLog> ConfigurationAuditLogs { get; set; }
    public DbSet<ConfigurationSnapshot> ConfigurationSnapshots { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Configuration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Value).IsRequired();
            entity.Property(e => e.Environment).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Application).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Tags).HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
            
            entity.HasIndex(e => new { e.Key, e.Environment, e.Application, e.TenantId }).IsUnique();
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => new { e.Environment, e.Application });
        });

        modelBuilder.Entity<ConfigurationValidationRule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RuleType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.RuleExpression).IsRequired();
            entity.Property(e => e.ErrorMessage).IsRequired().HasMaxLength(500);
            entity.Property(e => e.AllowedValues).HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
            
            entity.HasOne<Configuration>()
                .WithOne(c => c.ValidationRule)
                .HasForeignKey<ConfigurationValidationRule>(r => r.ConfigurationId);
        });

        modelBuilder.Entity<FeatureFlag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Environment).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Application).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Metadata).HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, object>());
            
            entity.HasIndex(e => new { e.Name, e.Environment, e.Application, e.TenantId }).IsUnique();
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => new { e.Environment, e.Application });
        });

        modelBuilder.Entity<FeatureFlagRule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RuleType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Attribute).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Operator).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Value).IsRequired();
            
            entity.HasOne<FeatureFlag>()
                .WithMany(f => f.Rules)
                .HasForeignKey(r => r.FeatureFlagId);
        });

        modelBuilder.Entity<FeatureFlagSchedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TimeZone).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CronExpression).HasMaxLength(100);
            
            entity.HasOne<FeatureFlag>()
                .WithOne(f => f.Schedule)
                .HasForeignKey<FeatureFlagSchedule>(s => s.FeatureFlagId);
        });

        modelBuilder.Entity<ConfigurationDeployment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Environment).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Application).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DeployedBy).HasMaxLength(100);
            entity.Property(e => e.RollbackReason).HasMaxLength(500);
            entity.Property(e => e.Metadata).HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, object>());
            
            entity.HasIndex(e => new { e.Environment, e.Application });
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<ConfigurationDeploymentItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            
            entity.HasOne<ConfigurationDeployment>()
                .WithMany(d => d.Items)
                .HasForeignKey(i => i.DeploymentId);
        });

        modelBuilder.Entity<ConfigurationAuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UserName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.IpAddress).IsRequired().HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.AdditionalData).HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, object>());
            
            entity.HasIndex(e => e.ConfigurationId);
            entity.HasIndex(e => e.FeatureFlagId);
            entity.HasIndex(e => e.DeploymentId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.TenantId);
        });

        modelBuilder.Entity<ConfigurationSnapshot>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Environment).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Application).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.ConfigurationData).HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, string>());
            entity.Property(e => e.FeatureFlagData).HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, bool>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, bool>());
            
            entity.HasIndex(e => new { e.Environment, e.Application });
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
