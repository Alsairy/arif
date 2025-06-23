using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Arif.Platform.Shared.Domain.Entities;
using Arif.Platform.Shared.Infrastructure.Services;

namespace Arif.Platform.Shared.Infrastructure.Data;

public class ArifPlatformDbContext : DbContext, IDataProtectionKeyContext
{
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ArifPlatformDbContext(
        DbContextOptions<ArifPlatformDbContext> options,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider) : base(options)
    {
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<Chatbot> Chatbots { get; set; }
    public DbSet<ChatSession> ChatSessions { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<GdprConsent> GdprConsents { get; set; }
    public DbSet<GdprDataProcessing> GdprDataProcessing { get; set; }
    public DbSet<SecurityAlertEntity> SecurityAlerts { get; set; }
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUserEntity(modelBuilder);
        ConfigureTenantEntity(modelBuilder);
        ConfigureRoleEntity(modelBuilder);
        ConfigurePermissionEntity(modelBuilder);
        ConfigureUserRoleEntity(modelBuilder);
        ConfigureRolePermissionEntity(modelBuilder);
        ConfigureChatbotEntity(modelBuilder);
        ConfigureChatSessionEntity(modelBuilder);
        ConfigureChatMessageEntity(modelBuilder);

        ApplyGlobalQueryFilters(modelBuilder);

        SeedInitialData(modelBuilder);
    }

    private void ConfigureUserEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();
            
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.PreferredLanguage).HasMaxLength(10).HasDefaultValue("ar");

            entity.HasOne(e => e.Tenant)
                  .WithMany(t => t.Users)
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureTenantEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Subdomain).IsUnique();
            
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Subdomain).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.LogoUrl).HasMaxLength(255);
            entity.Property(e => e.DefaultLanguage).HasMaxLength(10).HasDefaultValue("ar");
            entity.Property(e => e.TimeZone).HasMaxLength(10).HasDefaultValue("Asia/Riyadh");
            entity.Property(e => e.SubscriptionPlan).HasMaxLength(50).HasDefaultValue("free");
        });
    }

    private void ConfigureRoleEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(255);
        });
    }

    private void ConfigurePermissionEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
        });
    }

    private void ConfigureUserRoleEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();

            entity.HasOne(e => e.User)
                  .WithMany(u => u.UserRoles)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Role)
                  .WithMany(r => r.UserRoles)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureRolePermissionEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();

            entity.HasOne(e => e.Role)
                  .WithMany(r => r.RolePermissions)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Permission)
                  .WithMany(p => p.RolePermissions)
                  .HasForeignKey(e => e.PermissionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureChatbotEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Chatbot>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.Name }).IsUnique();
            
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Language).HasMaxLength(10).HasDefaultValue("ar");
            entity.Property(e => e.Configuration).HasDefaultValue("{}");
            entity.Property(e => e.KnowledgeBase).HasDefaultValue("{}");
            entity.Property(e => e.AvatarUrl).HasMaxLength(255);
            entity.Property(e => e.WelcomeMessage).HasMaxLength(100).HasDefaultValue("مرحباً! كيف يمكنني مساعدتك؟");

            entity.HasOne(e => e.Tenant)
                  .WithMany(t => t.Chatbots)
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureChatSessionEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SessionId).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.ChatbotId });
            
            entity.Property(e => e.SessionId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UserIdentifier).HasMaxLength(255);
            entity.Property(e => e.Channel).HasMaxLength(50).HasDefaultValue("web");
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("active");
            entity.Property(e => e.Metadata).HasDefaultValue("{}");

            entity.HasOne(e => e.Tenant)
                  .WithMany()
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Chatbot)
                  .WithMany(c => c.ChatSessions)
                  .HasForeignKey(e => e.ChatbotId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureChatMessageEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.SessionId, e.CreatedAt });
            
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.MessageType).HasMaxLength(20).HasDefaultValue("text");
            entity.Property(e => e.Sender).HasMaxLength(20).HasDefaultValue("user");

            entity.HasOne(e => e.Tenant)
                  .WithMany()
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Session)
                  .WithMany(s => s.Messages)
                  .HasForeignKey(e => e.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ApplyGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<Chatbot>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<ChatSession>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<ChatMessage>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Tenant>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Role>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Permission>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserRole>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<RolePermission>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Chatbot>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ChatSession>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ChatMessage>().HasQueryFilter(e => !e.IsDeleted);
    }

    private void SeedInitialData(ModelBuilder modelBuilder)
    {
        var adminRoleId = Guid.NewGuid();
        var userRoleId = Guid.NewGuid();
        var agentRoleId = Guid.NewGuid();

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = adminRoleId, Name = "Admin", Description = "System Administrator", IsSystemRole = true, CreatedAt = DateTime.UtcNow },
            new Role { Id = userRoleId, Name = "User", Description = "Regular User", IsSystemRole = true, CreatedAt = DateTime.UtcNow },
            new Role { Id = agentRoleId, Name = "Agent", Description = "Live Chat Agent", IsSystemRole = true, CreatedAt = DateTime.UtcNow }
        );

        var permissions = new[]
        {
            new Permission { Id = Guid.NewGuid(), Name = "users.read", Description = "Read users", Category = "Users", CreatedAt = DateTime.UtcNow },
            new Permission { Id = Guid.NewGuid(), Name = "users.write", Description = "Create/Update users", Category = "Users", CreatedAt = DateTime.UtcNow },
            new Permission { Id = Guid.NewGuid(), Name = "users.delete", Description = "Delete users", Category = "Users", CreatedAt = DateTime.UtcNow },
            new Permission { Id = Guid.NewGuid(), Name = "chatbots.read", Description = "Read chatbots", Category = "Chatbots", CreatedAt = DateTime.UtcNow },
            new Permission { Id = Guid.NewGuid(), Name = "chatbots.write", Description = "Create/Update chatbots", Category = "Chatbots", CreatedAt = DateTime.UtcNow },
            new Permission { Id = Guid.NewGuid(), Name = "chatbots.delete", Description = "Delete chatbots", Category = "Chatbots", CreatedAt = DateTime.UtcNow },
            new Permission { Id = Guid.NewGuid(), Name = "analytics.read", Description = "Read analytics", Category = "Analytics", CreatedAt = DateTime.UtcNow },
            new Permission { Id = Guid.NewGuid(), Name = "settings.write", Description = "Update settings", Category = "Settings", CreatedAt = DateTime.UtcNow }
        };

        modelBuilder.Entity<Permission>().HasData(permissions);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;
            var now = _dateTimeProvider.UtcNow;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = now;
                
                if (entity is ITenantAware tenantAware && tenantAware.TenantId == Guid.Empty)
                {
                    tenantAware.TenantId = _tenantContext.TenantId;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.UpdatedAt = now;
            }
        }
    }
}
