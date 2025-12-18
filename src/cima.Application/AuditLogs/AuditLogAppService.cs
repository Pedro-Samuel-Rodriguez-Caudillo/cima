using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.AuditLogging;

namespace cima.AuditLogs;

[Authorize(Roles = "admin")]
public class AuditLogAppService : ApplicationService, IAuditLogAppService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditLogAppService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<PagedResultDto<AuditLogDto>> GetListAsync(GetAuditLogsInput input)
    {
        var totalCount = await _auditLogRepository.GetCountAsync(
            startTime: input.StartTime,
            endTime: input.EndTime,
            httpMethod: input.HttpMethod,
            url: input.Url,
            userName: input.UserName,
            hasException: input.HasException,
            minExecutionDuration: input.MinExecutionDuration,
            maxExecutionDuration: input.MaxExecutionDuration
        );

        var auditLogs = await _auditLogRepository.GetListAsync(
            sorting: "ExecutionTime DESC",
            maxResultCount: input.MaxResultCount,
            skipCount: input.SkipCount,
            startTime: input.StartTime,
            endTime: input.EndTime,
            httpMethod: input.HttpMethod,
            url: input.Url,
            userName: input.UserName,
            hasException: input.HasException,
            minExecutionDuration: input.MinExecutionDuration,
            maxExecutionDuration: input.MaxExecutionDuration,
            includeDetails: false
        );

        var items = auditLogs.Select(x => new AuditLogDto
        {
            Id = x.Id,
            UserName = x.UserName,
            UserId = x.UserId,
            HttpMethod = x.HttpMethod,
            Url = x.Url,
            HttpStatusCode = x.HttpStatusCode,
            BrowserInfo = x.BrowserInfo,
            ClientIpAddress = x.ClientIpAddress,
            ExecutionTime = x.ExecutionTime,
            ExecutionDuration = x.ExecutionDuration,
            ApplicationName = x.ApplicationName
        }).ToList();

        return new PagedResultDto<AuditLogDto>(totalCount, items);
    }

    public async Task<AuditLogDto> GetAsync(Guid id)
    {
        var auditLog = await _auditLogRepository.GetAsync(id, includeDetails: true);

        var dto = new AuditLogDto
        {
            Id = auditLog.Id,
            UserName = auditLog.UserName,
            UserId = auditLog.UserId,
            HttpMethod = auditLog.HttpMethod,
            Url = auditLog.Url,
            HttpStatusCode = auditLog.HttpStatusCode,
            BrowserInfo = auditLog.BrowserInfo,
            ClientIpAddress = auditLog.ClientIpAddress,
            ExecutionTime = auditLog.ExecutionTime,
            ExecutionDuration = auditLog.ExecutionDuration,
            ApplicationName = auditLog.ApplicationName,
            Actions = auditLog.Actions.Select(a => new AuditLogActionDto
            {
                ServiceName = a.ServiceName,
                MethodName = a.MethodName,
                Parameters = a.Parameters,
                ExecutionTime = a.ExecutionTime,
                ExecutionDuration = a.ExecutionDuration
            }).ToList(),
            EntityChanges = auditLog.EntityChanges.Select(e => new EntityChangeDto
            {
                Id = e.Id,
                EntityTypeFullName = e.EntityTypeFullName,
                EntityId = e.EntityId,
                ChangeType = e.ChangeType.ToString(),
                ChangeTime = e.ChangeTime,
                PropertyChanges = e.PropertyChanges.Select(p => new PropertyChangeDto
                {
                    PropertyName = p.PropertyName,
                    OriginalValue = p.OriginalValue,
                    NewValue = p.NewValue
                }).ToList()
            }).ToList()
        };

        return dto;
    }
}
