using System.Collections.Concurrent;
using MultiTenantSaaS.Application.Contracts.Security;

namespace MultiTenantSaaS.Infrastructure.Security;

public class InMemoryRateLimitService : IRateLimitService
{
    private readonly ConcurrentDictionary<string, WindowCounter> _counters = new();

    public void Reset() => _counters.Clear();

    public Task<RateLimitResult> CheckAndIncrementAsync(
        string key,
        int permitLimit,
        TimeSpan window,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var counter = _counters.AddOrUpdate(
            key,
            _ => new WindowCounter(1, now.Add(window)),
            (_, existing) =>
            {
                if (existing.ExpiresAt <= now)
                {
                    return new WindowCounter(1, now.Add(window));
                }

                return existing with { Count = existing.Count + 1 };
            });

        if (counter.Count > permitLimit)
        {
            var retryAfter = Math.Max(1, (int)Math.Ceiling((counter.ExpiresAt - now).TotalSeconds));
            return Task.FromResult(new RateLimitResult(false, retryAfter));
        }

        return Task.FromResult(new RateLimitResult(true, 0));
    }

    private sealed record WindowCounter(int Count, DateTimeOffset ExpiresAt);
}
