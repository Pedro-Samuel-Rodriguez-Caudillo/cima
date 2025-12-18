using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace cima.AuditLogs;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public string? UserName { get; set; }
    public Guid? UserId { get; set; }
    public string? HttpMethod { get; set; }
    public string? Url { get; set; }
    public int? HttpStatusCode { get; set; }
    public string? BrowserInfo { get; set; }
    public string? ClientIpAddress { get; set; }
    public DateTime ExecutionTime { get; set; }
    public int ExecutionDuration { get; set; }
    public string? ApplicationName { get; set; }
    public List<AuditLogActionDto> Actions { get; set; } = new();
    public List<EntityChangeDto> EntityChanges { get; set; } = new();
}

public class AuditLogActionDto
{
    public string? ServiceName { get; set; }
    public string? MethodName { get; set; }
    public string? Parameters { get; set; }
    public DateTime ExecutionTime { get; set; }
    public int ExecutionDuration { get; set; }
}

public class EntityChangeDto
{
    public Guid Id { get; set; }
    public string? EntityTypeFullName { get; set; }
    public string? EntityId { get; set; }
    public string? ChangeType { get; set; }
    public DateTime ChangeTime { get; set; }
    public List<PropertyChangeDto> PropertyChanges { get; set; } = new();
}

public class PropertyChangeDto
{
    public string? PropertyName { get; set; }
    public string? OriginalValue { get; set; }
    public string? NewValue { get; set; }
}

public class GetAuditLogsInput : PagedAndSortedResultRequestDto
{
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? UserName { get; set; }
    public string? HttpMethod { get; set; }
    public string? Url { get; set; }
    public int? MinExecutionDuration { get; set; }
    public int? MaxExecutionDuration { get; set; }
    public bool? HasException { get; set; }
}
