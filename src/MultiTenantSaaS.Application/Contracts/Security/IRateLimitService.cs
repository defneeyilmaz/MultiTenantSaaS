namespace MultiTenantSaaS.Application.Contracts.Security;

public sealed record RateLimitResult(bool IsAllowed, int RetryAfterSeconds);

public interface IRateLimitService
{
    Task<RateLimitResult> CheckAndIncrementAsync(
        string key,
        int permitLimit,
        TimeSpan window,
        CancellationToken cancellationToken = default);
}
