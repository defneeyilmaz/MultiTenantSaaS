using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MultiTenantSaaS.Infrastructure.Persistence;
using MultiTenantSaaS.Infrastructure.Persistence.Seed;
using MultiTenantSaaS.Shared.Constants;
using Xunit;

namespace MultiTenantSaaS.IntegrationTests;

public class AuthorizationSeedTests : IClassFixture<IntegrationTestWebApplicationFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebApplicationFactory _factory;

    public AuthorizationSeedTests(IntegrationTestWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.InitializeDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task SeedData_CreatesRolesPermissionsAndMappings()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var roles = await dbContext.AppRoles
            .AsNoTracking()
            .Select(r => r.Name)
            .ToListAsync();

        Assert.Contains(RoleNames.PlatformAdmin, roles);
        Assert.Contains(RoleNames.TenantAdmin, roles);
        Assert.Contains(RoleNames.Manager, roles);
        Assert.Contains(RoleNames.Employee, roles);

        var tenantAdminPermissions = await dbContext.RolePermissions
            .AsNoTracking()
            .Where(rp => rp.RoleId == AuthorizationSeedData.TenantAdminRoleId)
            .Join(
                dbContext.Permissions.AsNoTracking(),
                rp => rp.PermissionId,
                p => p.Id,
                (_, p) => p.Name)
            .ToListAsync();

        Assert.Contains(PermissionNames.UsersInvite, tenantAdminPermissions);
        Assert.Contains(PermissionNames.AuditView, tenantAdminPermissions);
        Assert.DoesNotContain(PermissionNames.TenantsManage, tenantAdminPermissions);
    }
}
