using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Arif.Platform.IntegrationGateway.Infrastructure.Services;
using Arif.Platform.IntegrationGateway.Domain.Models;
using Arif.Platform.IntegrationGateway.Domain.Interfaces;
using Arif.Platform.Shared.Common.Security;

namespace Arif.Platform.IntegrationGateway.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnterpriseIntegrationsController : ControllerBase
{
    private readonly IEnterpriseIntegrationService _enterpriseIntegrationService;
    private readonly IAuditLogger _auditLogger;
    private readonly ILogger<EnterpriseIntegrationsController> _logger;

    public EnterpriseIntegrationsController(
        IEnterpriseIntegrationService enterpriseIntegrationService,
        IAuditLogger auditLogger,
        ILogger<EnterpriseIntegrationsController> logger)
    {
        _enterpriseIntegrationService = enterpriseIntegrationService;
        _auditLogger = auditLogger;
        _logger = logger;
    }

    #region SAP Integration Endpoints

    [HttpPost("sap/customers")]
    public async Task<IActionResult> CreateSAPCustomer([FromBody] CreateSAPCustomerRequest request)
    {
        try
        {
            var customer = await _enterpriseIntegrationService.CreateSAPCustomerAsync(request.Config, request.Customer);
            return Ok(new { success = true, customer });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create SAP customer");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("sap/customers/{customerNumber}")]
    public async Task<IActionResult> GetSAPCustomer([FromQuery] SAPConfiguration config, string customerNumber)
    {
        try
        {
            var customer = await _enterpriseIntegrationService.GetSAPCustomerAsync(config, customerNumber);
            return Ok(new { success = true, customer });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve SAP customer");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("sap/sales-orders")]
    public async Task<IActionResult> CreateSAPSalesOrder([FromBody] CreateSAPSalesOrderRequest request)
    {
        try
        {
            var order = await _enterpriseIntegrationService.CreateSAPSalesOrderAsync(request.Config, request.Order);
            return Ok(new { success = true, order });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create SAP sales order");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region Oracle Integration Endpoints

    [HttpPost("oracle/customers")]
    public async Task<IActionResult> CreateOracleCustomer([FromBody] CreateOracleCustomerRequest request)
    {
        try
        {
            var customer = await _enterpriseIntegrationService.CreateOracleCustomerAsync(request.Config, request.Customer);
            return Ok(new { success = true, customer });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Oracle customer");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("oracle/customers/{customerId}")]
    public async Task<IActionResult> GetOracleCustomer([FromQuery] OracleConfiguration config, int customerId)
    {
        try
        {
            var customer = await _enterpriseIntegrationService.GetOracleCustomerAsync(config, customerId);
            return Ok(new { success = true, customer });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Oracle customer");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("oracle/opportunities")]
    public async Task<IActionResult> CreateOracleOpportunity([FromBody] CreateOracleOpportunityRequest request)
    {
        try
        {
            var opportunity = await _enterpriseIntegrationService.CreateOracleOpportunityAsync(request.Config, request.Opportunity);
            return Ok(new { success = true, opportunity });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Oracle opportunity");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region Microsoft Dynamics Integration Endpoints

    [HttpPost("dynamics/accounts")]
    public async Task<IActionResult> CreateDynamicsAccount([FromBody] CreateDynamicsAccountRequest request)
    {
        try
        {
            var account = await _enterpriseIntegrationService.CreateDynamicsAccountAsync(request.Config, request.Account);
            return Ok(new { success = true, account });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Dynamics account");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("dynamics/accounts/{accountId}")]
    public async Task<IActionResult> GetDynamicsAccount([FromQuery] DynamicsConfiguration config, Guid accountId)
    {
        try
        {
            var account = await _enterpriseIntegrationService.GetDynamicsAccountAsync(config, accountId);
            return Ok(new { success = true, account });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Dynamics account");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("dynamics/contacts")]
    public async Task<IActionResult> CreateDynamicsContact([FromBody] CreateDynamicsContactRequest request)
    {
        try
        {
            var contact = await _enterpriseIntegrationService.CreateDynamicsContactAsync(request.Config, request.Contact);
            return Ok(new { success = true, contact });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Dynamics contact");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("dynamics/leads")]
    public async Task<IActionResult> CreateDynamicsLead([FromBody] CreateDynamicsLeadRequest request)
    {
        try
        {
            var lead = await _enterpriseIntegrationService.CreateDynamicsLeadAsync(request.Config, request.Lead);
            return Ok(new { success = true, lead });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Dynamics lead");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region ServiceNow Integration Endpoints

    [HttpPost("servicenow/incidents")]
    public async Task<IActionResult> CreateServiceNowIncident([FromBody] CreateServiceNowIncidentRequest request)
    {
        try
        {
            var incident = await _enterpriseIntegrationService.CreateServiceNowIncidentAsync(request.Config, request.Incident);
            return Ok(new { success = true, incident });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create ServiceNow incident");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("servicenow/incidents/{incidentNumber}")]
    public async Task<IActionResult> GetServiceNowIncident([FromQuery] ServiceNowConfiguration config, string incidentNumber)
    {
        try
        {
            var incident = await _enterpriseIntegrationService.GetServiceNowIncidentAsync(config, incidentNumber);
            return Ok(new { success = true, incident });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ServiceNow incident");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("servicenow/users/{userId}")]
    public async Task<IActionResult> GetServiceNowUser([FromQuery] ServiceNowConfiguration config, string userId)
    {
        try
        {
            var user = await _enterpriseIntegrationService.GetServiceNowUserAsync(config, userId);
            return Ok(new { success = true, user });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ServiceNow user");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region Zendesk Integration Endpoints

    [HttpPost("zendesk/tickets")]
    public async Task<IActionResult> CreateZendeskTicket([FromBody] CreateZendeskTicketRequest request)
    {
        try
        {
            var ticket = await _enterpriseIntegrationService.CreateZendeskTicketAsync(request.Config, request.Ticket);
            return Ok(new { success = true, ticket });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Zendesk ticket");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("zendesk/tickets/{ticketId}")]
    public async Task<IActionResult> GetZendeskTicket([FromQuery] ZendeskConfiguration config, long ticketId)
    {
        try
        {
            var ticket = await _enterpriseIntegrationService.GetZendeskTicketAsync(config, ticketId);
            return Ok(new { success = true, ticket });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Zendesk ticket");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("zendesk/users/{userId}")]
    public async Task<IActionResult> GetZendeskUser([FromQuery] ZendeskConfiguration config, long userId)
    {
        try
        {
            var user = await _enterpriseIntegrationService.GetZendeskUserAsync(config, userId);
            return Ok(new { success = true, user });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Zendesk user");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region Jira Integration Endpoints

    [HttpPost("jira/issues")]
    public async Task<IActionResult> CreateJiraIssue([FromBody] CreateJiraIssueRequest request)
    {
        try
        {
            var issue = await _enterpriseIntegrationService.CreateJiraIssueAsync(request.Config, request.Issue);
            return Ok(new { success = true, issue });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Jira issue");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("jira/issues/{issueKey}")]
    public async Task<IActionResult> GetJiraIssue([FromQuery] JiraConfiguration config, string issueKey)
    {
        try
        {
            var issue = await _enterpriseIntegrationService.GetJiraIssueAsync(config, issueKey);
            return Ok(new { success = true, issue });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Jira issue");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("jira/projects/{projectKey}")]
    public async Task<IActionResult> GetJiraProject([FromQuery] JiraConfiguration config, string projectKey)
    {
        try
        {
            var project = await _enterpriseIntegrationService.GetJiraProjectAsync(config, projectKey);
            return Ok(new { success = true, project });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Jira project");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region Common Integration Endpoints

    [HttpPost("sync")]
    public async Task<IActionResult> SyncData([FromBody] SyncDataRequest request)
    {
        try
        {
            var result = await _enterpriseIntegrationService.SyncDataAsync(request.SourceSystem, request.TargetSystem, request.SyncOptions);
            return Ok(new { success = true, result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync data");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("webhooks/{system}")]
    public async Task<IActionResult> ProcessWebhook(string system, [FromBody] WebhookPayload payload)
    {
        try
        {
            var processed = await _enterpriseIntegrationService.ProcessWebhookAsync(system, payload);
            return Ok(new { success = processed });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process webhook for {System}", system);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    #endregion
}

public class CreateSAPCustomerRequest
{
    public SAPConfiguration Config { get; set; } = new();
    public SAPCustomer Customer { get; set; } = new();
}

public class CreateSAPSalesOrderRequest
{
    public SAPConfiguration Config { get; set; } = new();
    public SAPSalesOrder Order { get; set; } = new();
}

public class CreateOracleCustomerRequest
{
    public OracleConfiguration Config { get; set; } = new();
    public OracleCustomer Customer { get; set; } = new();
}

public class CreateOracleOpportunityRequest
{
    public OracleConfiguration Config { get; set; } = new();
    public OracleOpportunity Opportunity { get; set; } = new();
}

public class CreateDynamicsAccountRequest
{
    public DynamicsConfiguration Config { get; set; } = new();
    public DynamicsAccount Account { get; set; } = new();
}

public class CreateDynamicsContactRequest
{
    public DynamicsConfiguration Config { get; set; } = new();
    public DynamicsContact Contact { get; set; } = new();
}

public class CreateDynamicsLeadRequest
{
    public DynamicsConfiguration Config { get; set; } = new();
    public DynamicsLead Lead { get; set; } = new();
}

public class CreateServiceNowIncidentRequest
{
    public ServiceNowConfiguration Config { get; set; } = new();
    public ServiceNowIncident Incident { get; set; } = new();
}

public class CreateZendeskTicketRequest
{
    public ZendeskConfiguration Config { get; set; } = new();
    public ZendeskTicket Ticket { get; set; } = new();
}

public class CreateJiraIssueRequest
{
    public JiraConfiguration Config { get; set; } = new();
    public JiraIssue Issue { get; set; } = new();
}

public class SyncDataRequest
{
    public string SourceSystem { get; set; } = string.Empty;
    public string TargetSystem { get; set; } = string.Empty;
    public Dictionary<string, object> SyncOptions { get; set; } = new();
}
