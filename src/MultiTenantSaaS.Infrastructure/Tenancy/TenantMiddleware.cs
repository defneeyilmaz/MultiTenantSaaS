using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MultiTenantSaaS.Application.Contracts.Tenancy;
using MultiTenantSaaS.Infrastructure.Persistence;
using MultiTenantSaaS.Shared.Constants;
using MultiTenantSaaS.Shared.Utilities;

namespace MultiTenantSaaS.Infrastructure.Tenancy;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsPublicPath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        string tenantSlug;
        try
        {
            var resolvedSlug = ResolveTenantSlug(context);
            if (string.IsNullOrWhiteSpace(resolvedSlug))
            {
                await WriteProblemAsync(
                    context,
                    StatusCodes.Status400BadRequest,
                    "Tenant is required",
                    "Provide the tenant slug via the X-Tenant-Slug header or subdomain.");
                return;
            }

            tenantSlug = SlugGenerator.From(resolvedSlug);
        }
        catch (ArgumentException)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Invalid tenant",
                "The tenant slug is invalid.");
            return;
        }

        var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();
        var tenant = await dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Slug == tenantSlug);

        if (tenant is null || !tenant.IsActive)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status403Forbidden,
                "Tenant access denied",
                "The tenant does not exist or is inactive.");
            return;
        }

        var tokenTenantId = context.User.FindFirstValue(AppConstants.TenantIdClaim);
        var tokenTenantSlug = context.User.FindFirstValue(AppConstants.TenantSlugClaim);

        if (tokenTenantId != tenant.Id.ToString()
            || (tokenTenantSlug is not null
                && !string.Equals(tokenTenantSlug, tenant.Slug, StringComparison.OrdinalIgnoreCase)))
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status403Forbidden,
                "Tenant mismatch",
                "The request tenant does not match the authenticated tenant.");
            return;
        }

        var userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue(ClaimTypes.Name)
            ?? context.User.FindFirstValue("sub");

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "The authenticated user could not be resolved.");
            return;
        }

        var hasMembership = await dbContext.UserTenantMemberships
            .AsNoTracking()
            .AnyAsync(m => m.UserId == userId && m.TenantId == tenant.Id && m.IsActive);

        if (!hasMembership)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status403Forbidden,
                "Tenant access denied",
                "The user is not an active member of this tenant.");
            return;
        }

        var tenantContext = context.RequestServices.GetRequiredService<ITenantContext>();
        tenantContext.SetTenant(tenant.Id, tenant.Slug);

        await _next(context);
    }

    private static bool IsPublicPath(PathString path)
    {
        if (path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return path.StartsWithSegments("/api/auth", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/api/users/accept-invitation", StringComparison.OrdinalIgnoreCase);
    }

    private static string? ResolveTenantSlug(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(AppConstants.TenantSlugHeader, out var headerValue)
            && !string.IsNullOrWhiteSpace(headerValue))
        {
            return headerValue.ToString();
        }

        var host = context.Request.Host.Host;
        var parts = host.Split('.', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length >= 3)
        {
            return parts[0];
        }

        if (parts.Length == 2 && parts[1].Equals("localhost", StringComparison.OrdinalIgnoreCase))
        {
            return parts[0];
        }

        return null;
    }

    private static async Task WriteProblemAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            title,
            status = statusCode,
            detail
        });
    }
}
