using Microsoft.Extensions.Logging;
using Arif.Platform.ConfigurationManagement.Domain.Entities;
using Arif.Platform.ConfigurationManagement.Domain.Interfaces;
using Arif.Platform.ConfigurationManagement.Domain.DTOs;

namespace Arif.Platform.ConfigurationManagement.Infrastructure.Services;

public class ConfigurationDeploymentService : IConfigurationDeploymentService
{
    private readonly IConfigurationDeploymentRepository _deploymentRepository;
    private readonly IConfigurationRepository _configurationRepository;
    private readonly IConfigurationAuditService _auditService;
    private readonly ILogger<ConfigurationDeploymentService> _logger;

    public ConfigurationDeploymentService(
        IConfigurationDeploymentRepository deploymentRepository,
        IConfigurationRepository configurationRepository,
        IConfigurationAuditService auditService,
        ILogger<ConfigurationDeploymentService> logger)
    {
        _deploymentRepository = deploymentRepository;
        _configurationRepository = configurationRepository;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<ConfigurationDeploymentDto> CreateDeploymentAsync(CreateDeploymentDto dto)
    {
        var deployment = new ConfigurationDeployment
        {
            Name = dto.Name,
            Description = dto.Description,
            Environment = dto.Environment,
            Application = dto.Application,
            CreatedBy = "system",
            Metadata = dto.Metadata,
            Items = dto.Items.Select(i => new ConfigurationDeploymentItem
            {
                ConfigurationId = i.ConfigurationId,
                Action = i.Action,
                NewValue = i.NewValue
            }).ToList()
        };

        var createdDeployment = await _deploymentRepository.CreateAsync(deployment);
        
        await _auditService.LogDeploymentActionAsync(createdDeployment.Id, "CREATE", createdDeployment.CreatedBy);

        _logger.LogInformation("Deployment created: {Name} for {Environment}/{Application}", 
            createdDeployment.Name, createdDeployment.Environment, createdDeployment.Application);

        return MapToDto(createdDeployment);
    }

    public async Task<ConfigurationDeploymentDto> GetDeploymentAsync(Guid id)
    {
        var deployment = await _deploymentRepository.GetByIdAsync(id);
        if (deployment == null)
            throw new KeyNotFoundException($"Deployment with ID '{id}' not found");

        return MapToDto(deployment);
    }

    public async Task<List<ConfigurationDeploymentDto>> GetDeploymentsAsync(string environment, string application)
    {
        var deployments = await _deploymentRepository.GetAllAsync(environment, application);
        return deployments.Select(MapToDto).ToList();
    }

    public async Task<bool> ExecuteDeploymentAsync(Guid id)
    {
        var deployment = await _deploymentRepository.GetByIdAsync(id);
        if (deployment == null)
            return false;

        if (deployment.Status != ConfigurationDeploymentStatus.Pending)
        {
            _logger.LogWarning("Deployment {Id} is not in pending status: {Status}", id, deployment.Status);
            return false;
        }

        deployment.Status = ConfigurationDeploymentStatus.InProgress;
        deployment.DeployedAt = DateTime.UtcNow;
        deployment.DeployedBy = "system";

        try
        {
            foreach (var item in deployment.Items)
            {
                item.Status = ConfigurationDeploymentItemStatus.Processing;
                
                try
                {
                    var configuration = await _configurationRepository.GetByIdAsync(item.ConfigurationId);
                    if (configuration == null)
                    {
                        item.Status = ConfigurationDeploymentItemStatus.Failed;
                        item.ErrorMessage = "Configuration not found";
                        continue;
                    }

                    item.OldValue = configuration.Value;

                    switch (item.Action.ToUpper())
                    {
                        case "UPDATE":
                            if (!string.IsNullOrEmpty(item.NewValue))
                            {
                                configuration.Value = item.NewValue;
                                configuration.UpdatedBy = "deployment";
                                await _configurationRepository.UpdateAsync(configuration);
                            }
                            break;
                        case "DELETE":
                            await _configurationRepository.DeleteAsync(item.ConfigurationId);
                            break;
                        default:
                            item.Status = ConfigurationDeploymentItemStatus.Failed;
                            item.ErrorMessage = $"Unknown action: {item.Action}";
                            continue;
                    }

                    item.Status = ConfigurationDeploymentItemStatus.Completed;
                    item.ProcessedAt = DateTime.UtcNow;

                    await _auditService.LogConfigurationChangeAsync(
                        item.ConfigurationId, 
                        $"DEPLOYMENT_{item.Action}", 
                        item.OldValue, 
                        item.NewValue, 
                        "deployment");
                }
                catch (Exception ex)
                {
                    item.Status = ConfigurationDeploymentItemStatus.Failed;
                    item.ErrorMessage = ex.Message;
                    _logger.LogError(ex, "Failed to process deployment item {ItemId}", item.Id);
                }
            }

            var hasFailures = deployment.Items.Any(i => i.Status == ConfigurationDeploymentItemStatus.Failed);
            deployment.Status = hasFailures ? ConfigurationDeploymentStatus.Failed : ConfigurationDeploymentStatus.Completed;
            deployment.CompletedAt = DateTime.UtcNow;

            await _deploymentRepository.UpdateAsync(deployment);
            
            await _auditService.LogDeploymentActionAsync(id, "EXECUTE", deployment.DeployedBy ?? "system");

            _logger.LogInformation("Deployment executed: {Name} with status {Status}", 
                deployment.Name, deployment.Status);

            return !hasFailures;
        }
        catch (Exception ex)
        {
            deployment.Status = ConfigurationDeploymentStatus.Failed;
            deployment.CompletedAt = DateTime.UtcNow;
            await _deploymentRepository.UpdateAsync(deployment);
            
            _logger.LogError(ex, "Failed to execute deployment {Id}", id);
            return false;
        }
    }

    public async Task<bool> RollbackDeploymentAsync(Guid id, string reason)
    {
        var deployment = await _deploymentRepository.GetByIdAsync(id);
        if (deployment == null)
            return false;

        if (deployment.Status != ConfigurationDeploymentStatus.Completed)
        {
            _logger.LogWarning("Cannot rollback deployment {Id} with status {Status}", id, deployment.Status);
            return false;
        }

        try
        {
            foreach (var item in deployment.Items.Where(i => i.Status == ConfigurationDeploymentItemStatus.Completed))
            {
                var configuration = await _configurationRepository.GetByIdAsync(item.ConfigurationId);
                if (configuration != null && !string.IsNullOrEmpty(item.OldValue))
                {
                    configuration.Value = item.OldValue;
                    configuration.UpdatedBy = "rollback";
                    await _configurationRepository.UpdateAsync(configuration);

                    await _auditService.LogConfigurationChangeAsync(
                        item.ConfigurationId, 
                        "ROLLBACK", 
                        item.NewValue, 
                        item.OldValue, 
                        "rollback");
                }
            }

            deployment.Status = ConfigurationDeploymentStatus.RolledBack;
            deployment.RollbackReason = reason;
            await _deploymentRepository.UpdateAsync(deployment);
            
            await _auditService.LogDeploymentActionAsync(id, "ROLLBACK", "system");

            _logger.LogInformation("Deployment rolled back: {Name} - {Reason}", deployment.Name, reason);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rollback deployment {Id}", id);
            return false;
        }
    }

    public async Task<bool> CancelDeploymentAsync(Guid id)
    {
        var deployment = await _deploymentRepository.GetByIdAsync(id);
        if (deployment == null)
            return false;

        if (deployment.Status != ConfigurationDeploymentStatus.Pending && 
            deployment.Status != ConfigurationDeploymentStatus.InProgress)
        {
            return false;
        }

        deployment.Status = ConfigurationDeploymentStatus.Cancelled;
        deployment.CompletedAt = DateTime.UtcNow;
        await _deploymentRepository.UpdateAsync(deployment);
        
        await _auditService.LogDeploymentActionAsync(id, "CANCEL", "system");

        _logger.LogInformation("Deployment cancelled: {Name}", deployment.Name);
        return true;
    }

    public async Task<ConfigurationDeploymentStatus> GetDeploymentStatusAsync(Guid id)
    {
        var deployment = await _deploymentRepository.GetByIdAsync(id);
        return deployment?.Status ?? ConfigurationDeploymentStatus.Pending;
    }

    public async Task<List<ConfigurationDeploymentItem>> GetDeploymentItemsAsync(Guid deploymentId)
    {
        var deployment = await _deploymentRepository.GetByIdAsync(deploymentId);
        return deployment?.Items ?? new List<ConfigurationDeploymentItem>();
    }

    private static ConfigurationDeploymentDto MapToDto(ConfigurationDeployment deployment)
    {
        return new ConfigurationDeploymentDto
        {
            Id = deployment.Id,
            Name = deployment.Name,
            Description = deployment.Description,
            Environment = deployment.Environment,
            Application = deployment.Application,
            Status = deployment.Status.ToString(),
            CreatedAt = deployment.CreatedAt,
            DeployedAt = deployment.DeployedAt,
            CompletedAt = deployment.CompletedAt,
            CreatedBy = deployment.CreatedBy,
            DeployedBy = deployment.DeployedBy,
            RollbackReason = deployment.RollbackReason,
            RollbackToDeploymentId = deployment.RollbackToDeploymentId,
            Metadata = deployment.Metadata,
            Items = deployment.Items.Select(i => new ConfigurationDeploymentItemDto
            {
                Id = i.Id,
                ConfigurationId = i.ConfigurationId,
                Action = i.Action,
                OldValue = i.OldValue,
                NewValue = i.NewValue,
                Status = i.Status.ToString(),
                ErrorMessage = i.ErrorMessage,
                ProcessedAt = i.ProcessedAt
            }).ToList()
        };
    }
}
