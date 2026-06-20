using Microsoft.AspNetCore.Builder;

namespace MultiTenantSaaS.Infrastructure.Security;

public static class AuthRateLimitMiddlewareExtensions
{
    public static IApplicationBuilder UseAuthRateLimit(this IApplicationBuilder app) =>
        app.UseMiddleware<AuthRateLimitMiddleware>();
}
