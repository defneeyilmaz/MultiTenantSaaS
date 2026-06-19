namespace MultiTenantSaaS.Application.Contracts.Audit;

public interface IAuditService
{
    Task LogAsync(
        string action,
        string? details = null,
        string? entityType = null,
        Guid? entityId = null,
        Guid? tenantId = null,
        Guid? actorUserId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditLogDto>> ListAsync(CancellationToken cancellationToken = default);
}
