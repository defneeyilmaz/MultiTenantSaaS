using MultiTenantSaaS.Application.Contracts.Security;
using StackExchange.Redis;

namespace MultiTenantSaaS.Infrastructure.Security;

public class RedisRateLimitService : IRateLimitService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisRateLimitService(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task<RateLimitResult> CheckAndIncrementAsync(
        string key,
        int permitLimit,
        TimeSpan window,
        CancellationToken cancellationToken = default)
    {
        var database = _connectionMultiplexer.GetDatabase();
        var count = await database.StringIncrementAsync(key);

        if (count == 1)
        {
            await database.KeyExpireAsync(key, window);
        }

        if (count > permitLimit)
        {
            var ttl = await database.KeyTimeToLiveAsync(key);
            var retryAfter = ttl.HasValue && ttl.Value > TimeSpan.Zero
                ? Math.Max(1, (int)Math.Ceiling(ttl.Value.TotalSeconds))
                : Math.Max(1, (int)Math.Ceiling(window.TotalSeconds));

            return new RateLimitResult(false, retryAfter);
        }

        return new RateLimitResult(true, 0);
    }
}
