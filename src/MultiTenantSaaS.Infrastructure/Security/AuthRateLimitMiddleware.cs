using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MultiTenantSaaS.Application.Contracts.Security;
using MultiTenantSaaS.Infrastructure.Options;

namespace MultiTenantSaaS.Infrastructure.Security;

public class AuthRateLimitMiddleware
{
    private readonly RequestDelegate _next;

    public AuthRateLimitMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IRateLimitService rateLimitService,
        IOptions<RateLimitOptions> rateLimitOptions)
    {
        if (!HttpMethods.IsPost(context.Request.Method)
            || !context.Request.Path.StartsWithSegments("/api/auth", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var options = rateLimitOptions.Value;
        string key;
        int permitLimit;
        TimeSpan window;

        if (context.Request.Path.StartsWithSegments("/api/auth/login", StringComparison.OrdinalIgnoreCase))
        {
            key = $"auth:login:{ResolveClientIp(context)}";
            permitLimit = options.LoginPermitLimit;
            window = TimeSpan.FromSeconds(options.LoginWindowSeconds);
        }
        else if (context.Request.Path.StartsWithSegments("/api/auth/refresh-token", StringComparison.OrdinalIgnoreCase))
        {
            key = $"auth:refresh:{ResolveClientIp(context)}";
            permitLimit = options.RefreshPermitLimit;
            window = TimeSpan.FromSeconds(options.RefreshWindowSeconds);
        }
        else
        {
            await _next(context);
            return;
        }

        var result = await rateLimitService.CheckAndIncrementAsync(
            key,
            permitLimit,
            window,
            context.RequestAborted);

        if (!result.IsAllowed)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers.RetryAfter = result.RetryAfterSeconds.ToString();
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc6585#section-4",
                title = "Too many requests",
                status = StatusCodes.Status429TooManyRequests,
                detail = "Too many authentication attempts. Try again later."
            });
            return;
        }

        await _next(context);
    }

    private static string ResolveClientIp(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
