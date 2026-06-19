namespace MultiTenantSaaS.Application.Contracts.Audit;

public sealed record AuditLogDto(
    Guid Id,
    Guid TenantId,
    Guid? ActorUserId,
    string? ActorEmail,
    string Action,
    string? EntityType,
    Guid? EntityId,
    string? Details,
    string? IpAddress,
    DateTimeOffset CreatedAt);
