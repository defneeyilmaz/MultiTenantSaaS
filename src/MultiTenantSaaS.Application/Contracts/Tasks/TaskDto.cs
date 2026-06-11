using MultiTenantSaaS.Domain.Enums;

namespace MultiTenantSaaS.Application.Contracts.Tasks;

public sealed record TaskDto(
    Guid Id,
    Guid ProjectId,
    string Title,
    string? Description,
    WorkTaskStatus Status,
    Guid? AssignedToUserId,
    DateTimeOffset CreatedAt);

public sealed record CreateTaskRequest(
    Guid ProjectId,
    string Title,
    string? Description,
    Guid? AssignedToUserId);

public sealed record UpdateTaskStatusRequest(WorkTaskStatus Status);
