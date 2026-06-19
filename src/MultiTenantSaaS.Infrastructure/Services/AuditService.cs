using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MultiTenantSaaS.Application.Contracts.Audit;
using MultiTenantSaaS.Application.Contracts.Tenancy;
using MultiTenantSaaS.Domain.Entities;
using MultiTenantSaaS.Infrastructure.Persistence;

namespace MultiTenantSaaS.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(
        AppDbContext dbContext,
        ITenantContext tenantContext,
        IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(
        string action,
        string? details = null,
        string? entityType = null,
        Guid? entityId = null,
        Guid? tenantId = null,
        Guid? actorUserId = null,
        CancellationToken cancellationToken = default)
    {
        var resolvedTenantId = tenantId ?? _tenantContext.TenantId;
        if (resolvedTenantId is null)
        {
            return;
        }

        var resolvedActorUserId = actorUserId ?? ResolveActorUserId();

        _dbContext.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = resolvedTenantId.Value,
            ActorUserId = resolvedActorUserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            IpAddress = ResolveIpAddress(),
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLogDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var logs = await _dbContext.AuditLogs
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var orderedLogs = logs
            .OrderByDescending(log => log.CreatedAt)
            .Take(100)
            .ToList();

        var actorIds = orderedLogs
            .Where(log => log.ActorUserId.HasValue)
            .Select(log => log.ActorUserId!.Value)
            .Distinct()
            .ToList();

        var actorEmails = actorIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _dbContext.Users
                .AsNoTracking()
                .Where(user => actorIds.Contains(user.Id))
                .ToDictionaryAsync(user => user.Id, user => user.Email ?? string.Empty, cancellationToken);

        return orderedLogs
            .Select(log => new AuditLogDto(
                log.Id,
                log.TenantId,
                log.ActorUserId,
                log.ActorUserId is null ? null : actorEmails.GetValueOrDefault(log.ActorUserId.Value),
                log.Action,
                log.EntityType,
                log.EntityId,
                log.Details,
                log.IpAddress,
                log.CreatedAt))
            .ToList();
    }

    private Guid? ResolveActorUserId()
    {
        var userIdValue = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name)
            ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("sub");

        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }

    private string? ResolveIpAddress() =>
        _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
