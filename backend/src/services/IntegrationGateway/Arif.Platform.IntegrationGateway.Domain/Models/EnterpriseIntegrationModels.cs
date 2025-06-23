using System.ComponentModel.DataAnnotations;

namespace Arif.Platform.IntegrationGateway.Domain.Models;

public class SAPConfiguration
{
    [Required]
    public string BaseUrl { get; set; } = string.Empty;
    
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    public string Client { get; set; } = string.Empty;
    
    public string SystemId { get; set; } = string.Empty;
    public string Language { get; set; } = "EN";
    public Dictionary<string, string> CustomFields { get; set; } = new();
}

public class SAPCustomer
{
    public string CustomerNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string CompanyCode { get; set; } = string.Empty;
    public Dictionary<string, object> CustomFields { get; set; } = new();
}

public class SAPSalesOrder
{
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<SAPOrderItem> Items { get; set; } = new();
}

public class SAPOrderItem
{
    public string ItemNumber { get; set; } = string.Empty;
    public string MaterialNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class OracleConfiguration
{
    [Required]
    public string BaseUrl { get; set; } = string.Empty;
    
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
    
    public string DatabaseName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public int Port { get; set; } = 1521;
    public Dictionary<string, string> ConnectionProperties { get; set; } = new();
}

public class OracleCustomer
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public Dictionary<string, object> Attributes { get; set; } = new();
}

public class OracleOpportunity
{
    public int OpportunityId { get; set; }
    public string OpportunityName { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public DateTime CloseDate { get; set; }
    public double Probability { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class DynamicsConfiguration
{
    [Required]
    public string OrganizationUrl { get; set; } = string.Empty;
    
    [Required]
    public string ClientId { get; set; } = string.Empty;
    
    [Required]
    public string ClientSecret { get; set; } = string.Empty;
    
    [Required]
    public string TenantId { get; set; } = string.Empty;
    
    public string Resource { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "v9.2";
}

public class DynamicsAccount
{
    public Guid AccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int NumberOfEmployees { get; set; }
    public Dictionary<string, object> CustomFields { get; set; } = new();
}

public class DynamicsContact
{
    public Guid ContactId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public Guid? ParentAccountId { get; set; }
    public Dictionary<string, object> CustomFields { get; set; } = new();
}

public class DynamicsLead
{
    public Guid LeadId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Rating { get; set; }
    public Dictionary<string, object> CustomFields { get; set; } = new();
}

public class ServiceNowConfiguration
{
    [Required]
    public string InstanceUrl { get; set; } = string.Empty;
    
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
    
    public string ApiVersion { get; set; } = "v1";
    public Dictionary<string, string> Headers { get; set; } = new();
}

public class ServiceNowIncident
{
    public string Number { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Subcategory { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Urgency { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
    public string CallerEmail { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public Dictionary<string, object> CustomFields { get; set; } = new();
}

public class ServiceNowUser
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool Active { get; set; }
}

public class ZendeskConfiguration
{
    [Required]
    public string Subdomain { get; set; } = string.Empty;
    
    [Required]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string ApiToken { get; set; } = string.Empty;
    
    public string ApiVersion { get; set; } = "v2";
}

public class ZendeskTicket
{
    public long Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public long RequesterId { get; set; }
    public long? AssigneeId { get; set; }
    public long? GroupId { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ZendeskComment> Comments { get; set; } = new();
}

public class ZendeskComment
{
    public long Id { get; set; }
    public string Body { get; set; } = string.Empty;
    public long AuthorId { get; set; }
    public bool Public { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ZendeskAttachment> Attachments { get; set; } = new();
}

public class ZendeskAttachment
{
    public long Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentUrl { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
}

public class ZendeskUser
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object> UserFields { get; set; } = new();
}

public class JiraConfiguration
{
    [Required]
    public string BaseUrl { get; set; } = string.Empty;
    
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string ApiToken { get; set; } = string.Empty;
    
    public string ProjectKey { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "3";
}

public class JiraIssue
{
    public string Key { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IssueType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string ProjectKey { get; set; } = string.Empty;
    public string AssigneeEmail { get; set; } = string.Empty;
    public string ReporterEmail { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? DueDate { get; set; }
    public List<string> Labels { get; set; } = new();
    public List<JiraComment> Comments { get; set; } = new();
    public Dictionary<string, object> CustomFields { get; set; } = new();
}

public class JiraComment
{
    public string Id { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

public class JiraProject
{
    public string Key { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ProjectTypeKey { get; set; } = string.Empty;
    public string Lead { get; set; } = string.Empty;
    public List<JiraIssueType> IssueTypes { get; set; } = new();
}

public class JiraIssueType
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Subtask { get; set; }
}

public class IntegrationSyncResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int RecordsProcessed { get; set; }
    public int RecordsSucceeded { get; set; }
    public int RecordsFailed { get; set; }
    public DateTime SyncTimestamp { get; set; }
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class WebhookPayload
{
    public string Source { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
    public string Signature { get; set; } = string.Empty;
}

public class IntegrationMapping
{
    public string SourceSystem { get; set; } = string.Empty;
    public string SourceField { get; set; } = string.Empty;
    public string TargetSystem { get; set; } = string.Empty;
    public string TargetField { get; set; } = string.Empty;
    public string TransformationRule { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public string DefaultValue { get; set; } = string.Empty;
}
