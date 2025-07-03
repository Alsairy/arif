using System.ComponentModel.DataAnnotations;

namespace Arif.Platform.WorkflowEngine.Domain.DTOs
{
    public class CreateWorkflowDefinitionRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        [Required]
        public WorkflowDefinitionData Definition { get; set; } = new();
        
        public Guid TenantId { get; set; }
        
        public Guid CreatedBy { get; set; }
        
        public string? Category { get; set; }
        
        public string[] Tags { get; set; } = Array.Empty<string>();
        
        public bool IsActive { get; set; } = true;
    }

    public class WorkflowDefinitionData
    {
        public WorkflowNode[] Nodes { get; set; } = Array.Empty<WorkflowNode>();
        
        public WorkflowConnection[] Connections { get; set; } = Array.Empty<WorkflowConnection>();
        
        public WorkflowVariable[] Variables { get; set; } = Array.Empty<WorkflowVariable>();
        
        public Dictionary<string, object>? Settings { get; set; }
        
        public string Version { get; set; } = "1.0";
    }

    public class WorkflowNode
    {
        public string Id { get; set; } = string.Empty;
        
        public string Type { get; set; } = string.Empty;
        
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public WorkflowNodePosition Position { get; set; } = new();
        
        public Dictionary<string, object>? Properties { get; set; }
        
        public WorkflowNodeCondition[] Conditions { get; set; } = Array.Empty<WorkflowNodeCondition>();
        
        public string[] InputPorts { get; set; } = Array.Empty<string>();
        
        public string[] OutputPorts { get; set; } = Array.Empty<string>();
    }

    public class WorkflowNodePosition
    {
        public double X { get; set; }
        
        public double Y { get; set; }
    }

    public class WorkflowNodeCondition
    {
        public string Field { get; set; } = string.Empty;
        
        public string Operator { get; set; } = string.Empty;
        
        public object? Value { get; set; }
        
        public string LogicalOperator { get; set; } = "AND";
    }

    public class WorkflowConnection
    {
        public string Id { get; set; } = string.Empty;
        
        public string SourceNodeId { get; set; } = string.Empty;
        
        public string SourcePort { get; set; } = string.Empty;
        
        public string TargetNodeId { get; set; } = string.Empty;
        
        public string TargetPort { get; set; } = string.Empty;
        
        public Dictionary<string, object>? Properties { get; set; }
    }

    public class WorkflowVariable
    {
        public string Name { get; set; } = string.Empty;
        
        public string Type { get; set; } = string.Empty;
        
        public object? DefaultValue { get; set; }
        
        public string? Description { get; set; }
        
        public bool IsRequired { get; set; }
        
        public string Scope { get; set; } = "workflow";
    }

    public class WorkflowDefinitionResponse
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public WorkflowDefinitionData Definition { get; set; } = new();
        
        public Guid TenantId { get; set; }
        
        public Guid CreatedBy { get; set; }
        
        public string? Category { get; set; }
        
        public string[] Tags { get; set; } = Array.Empty<string>();
        
        public bool IsActive { get; set; }
        
        public string Status { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
        
        public string Version { get; set; } = string.Empty;
    }

    public class GetWorkflowDefinitionsRequest
    {
        public Guid TenantId { get; set; }
        
        public int Page { get; set; } = 1;
        
        public int PageSize { get; set; } = 20;
        
        public string? Search { get; set; }
        
        public string? Category { get; set; }
        
        public string[]? Tags { get; set; }
        
        public bool? IsActive { get; set; }
    }

    public class GetWorkflowDefinitionsResponse
    {
        public WorkflowDefinitionResponse[] Items { get; set; } = Array.Empty<WorkflowDefinitionResponse>();
        
        public int TotalCount { get; set; }
        
        public int Page { get; set; }
        
        public int PageSize { get; set; }
        
        public int TotalPages { get; set; }
    }

    public class UpdateWorkflowDefinitionRequest
    {
        public Guid WorkflowId { get; set; }
        
        public Guid TenantId { get; set; }
        
        public Guid UpdatedBy { get; set; }
        
        public string? Name { get; set; }
        
        public string? Description { get; set; }
        
        public WorkflowDefinitionData? Definition { get; set; }
        
        public string? Category { get; set; }
        
        public string[]? Tags { get; set; }
        
        public bool? IsActive { get; set; }
    }

    public class ExecuteWorkflowRequest
    {
        [Required]
        public Guid WorkflowId { get; set; }
        
        public Guid TenantId { get; set; }
        
        public Guid ExecutedBy { get; set; }
        
        public Dictionary<string, object>? InputData { get; set; }
        
        public string? CorrelationId { get; set; }
        
        public Dictionary<string, object>? Context { get; set; }
    }

    public class WorkflowExecutionResponse
    {
        public Guid ExecutionId { get; set; }
        
        public Guid WorkflowId { get; set; }
        
        public string WorkflowName { get; set; } = string.Empty;
        
        public string Status { get; set; } = string.Empty;
        
        public DateTime StartedAt { get; set; }
        
        public DateTime? CompletedAt { get; set; }
        
        public Dictionary<string, object>? InputData { get; set; }
        
        public Dictionary<string, object>? OutputData { get; set; }
        
        public WorkflowExecutionStep[] Steps { get; set; } = Array.Empty<WorkflowExecutionStep>();
        
        public string? ErrorMessage { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class WorkflowExecutionStep
    {
        public string NodeId { get; set; } = string.Empty;
        
        public string NodeName { get; set; } = string.Empty;
        
        public string Status { get; set; } = string.Empty;
        
        public DateTime StartedAt { get; set; }
        
        public DateTime? CompletedAt { get; set; }
        
        public Dictionary<string, object>? InputData { get; set; }
        
        public Dictionary<string, object>? OutputData { get; set; }
        
        public string? ErrorMessage { get; set; }
        
        public int ExecutionOrder { get; set; }
    }

    public class GetWorkflowExecutionsRequest
    {
        public Guid TenantId { get; set; }
        
        public Guid? WorkflowId { get; set; }
        
        public string? Status { get; set; }
        
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        public int Page { get; set; } = 1;
        
        public int PageSize { get; set; } = 20;
    }

    public class GetWorkflowExecutionsResponse
    {
        public WorkflowExecutionResponse[] Items { get; set; } = Array.Empty<WorkflowExecutionResponse>();
        
        public int TotalCount { get; set; }
        
        public int Page { get; set; }
        
        public int PageSize { get; set; }
        
        public int TotalPages { get; set; }
    }

    public class WorkflowTemplate
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public string Category { get; set; } = string.Empty;
        
        public WorkflowDefinitionData Template { get; set; } = new();
        
        public string[] Tags { get; set; } = Array.Empty<string>();
        
        public string PreviewImage { get; set; } = string.Empty;
        
        public bool IsBuiltIn { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class ValidateWorkflowRequest
    {
        [Required]
        public WorkflowDefinitionData WorkflowDefinition { get; set; } = new();
        
        public Dictionary<string, object>? Context { get; set; }
    }

    public class ValidateWorkflowResponse
    {
        public bool IsValid { get; set; }
        
        public WorkflowValidationError[] Errors { get; set; } = Array.Empty<WorkflowValidationError>();
        
        public WorkflowValidationWarning[] Warnings { get; set; } = Array.Empty<WorkflowValidationWarning>();
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class WorkflowValidationError
    {
        public string Code { get; set; } = string.Empty;
        
        public string Message { get; set; } = string.Empty;
        
        public string? NodeId { get; set; }
        
        public string? Field { get; set; }
        
        public string Severity { get; set; } = "Error";
    }

    public class WorkflowValidationWarning
    {
        public string Code { get; set; } = string.Empty;
        
        public string Message { get; set; } = string.Empty;
        
        public string? NodeId { get; set; }
        
        public string? Field { get; set; }
        
        public string Severity { get; set; } = "Warning";
    }

    public class WorkflowStateMachine
    {
        public string CurrentState { get; set; } = string.Empty;
        
        public string[] AvailableTransitions { get; set; } = Array.Empty<string>();
        
        public Dictionary<string, object>? StateData { get; set; }
        
        public WorkflowStateHistory[] History { get; set; } = Array.Empty<WorkflowStateHistory>();
    }

    public class WorkflowStateHistory
    {
        public string FromState { get; set; } = string.Empty;
        
        public string ToState { get; set; } = string.Empty;
        
        public DateTime TransitionedAt { get; set; }
        
        public string? Reason { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
