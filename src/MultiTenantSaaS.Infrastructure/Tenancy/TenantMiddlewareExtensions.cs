using Microsoft.AspNetCore.Builder;

namespace MultiTenantSaaS.Infrastructure.Tenancy;

public static class TenantMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder app)
        => app.UseMiddleware<TenantMiddleware>();
}
