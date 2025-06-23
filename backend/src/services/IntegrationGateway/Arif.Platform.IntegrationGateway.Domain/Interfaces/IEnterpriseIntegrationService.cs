using Arif.Platform.IntegrationGateway.Domain.Models;

namespace Arif.Platform.IntegrationGateway.Domain.Interfaces;

public interface IEnterpriseIntegrationService
{
    Task<SAPCustomer> CreateSAPCustomerAsync(SAPConfiguration config, SAPCustomer customer, CancellationToken cancellationToken = default);
    Task<SAPCustomer?> GetSAPCustomerAsync(SAPConfiguration config, string customerNumber, CancellationToken cancellationToken = default);
    Task<SAPSalesOrder> CreateSAPSalesOrderAsync(SAPConfiguration config, SAPSalesOrder order, CancellationToken cancellationToken = default);
    Task<List<SAPSalesOrder>> GetSAPSalesOrdersAsync(SAPConfiguration config, string customerNumber, CancellationToken cancellationToken = default);
    
    Task<OracleCustomer> CreateOracleCustomerAsync(OracleConfiguration config, OracleCustomer customer, CancellationToken cancellationToken = default);
    Task<OracleCustomer?> GetOracleCustomerAsync(OracleConfiguration config, int customerId, CancellationToken cancellationToken = default);
    Task<OracleOpportunity> CreateOracleOpportunityAsync(OracleConfiguration config, OracleOpportunity opportunity, CancellationToken cancellationToken = default);
    Task<List<OracleOpportunity>> GetOracleOpportunitiesAsync(OracleConfiguration config, int customerId, CancellationToken cancellationToken = default);
    
    Task<DynamicsAccount> CreateDynamicsAccountAsync(DynamicsConfiguration config, DynamicsAccount account, CancellationToken cancellationToken = default);
    Task<DynamicsAccount?> GetDynamicsAccountAsync(DynamicsConfiguration config, Guid accountId, CancellationToken cancellationToken = default);
    Task<DynamicsContact> CreateDynamicsContactAsync(DynamicsConfiguration config, DynamicsContact contact, CancellationToken cancellationToken = default);
    Task<DynamicsLead> CreateDynamicsLeadAsync(DynamicsConfiguration config, DynamicsLead lead, CancellationToken cancellationToken = default);
    
    Task<ServiceNowIncident> CreateServiceNowIncidentAsync(ServiceNowConfiguration config, ServiceNowIncident incident, CancellationToken cancellationToken = default);
    Task<ServiceNowIncident?> GetServiceNowIncidentAsync(ServiceNowConfiguration config, string incidentNumber, CancellationToken cancellationToken = default);
    Task<ServiceNowUser?> GetServiceNowUserAsync(ServiceNowConfiguration config, string userId, CancellationToken cancellationToken = default);
    Task<List<ServiceNowIncident>> GetServiceNowIncidentsByUserAsync(ServiceNowConfiguration config, string userEmail, CancellationToken cancellationToken = default);
    
    Task<ZendeskTicket> CreateZendeskTicketAsync(ZendeskConfiguration config, ZendeskTicket ticket, CancellationToken cancellationToken = default);
    Task<ZendeskTicket?> GetZendeskTicketAsync(ZendeskConfiguration config, long ticketId, CancellationToken cancellationToken = default);
    Task<ZendeskUser?> GetZendeskUserAsync(ZendeskConfiguration config, long userId, CancellationToken cancellationToken = default);
    Task<List<ZendeskTicket>> GetZendeskTicketsByUserAsync(ZendeskConfiguration config, long userId, CancellationToken cancellationToken = default);
    
    Task<JiraIssue> CreateJiraIssueAsync(JiraConfiguration config, JiraIssue issue, CancellationToken cancellationToken = default);
    Task<JiraIssue?> GetJiraIssueAsync(JiraConfiguration config, string issueKey, CancellationToken cancellationToken = default);
    Task<List<JiraIssue>> GetJiraIssuesByProjectAsync(JiraConfiguration config, string projectKey, CancellationToken cancellationToken = default);
    Task<JiraProject?> GetJiraProjectAsync(JiraConfiguration config, string projectKey, CancellationToken cancellationToken = default);
    
    Task<IntegrationSyncResult> SyncDataAsync(string sourceSystem, string targetSystem, Dictionary<string, object> syncOptions, CancellationToken cancellationToken = default);
    Task<bool> ProcessWebhookAsync(string system, WebhookPayload payload, CancellationToken cancellationToken = default);
}
