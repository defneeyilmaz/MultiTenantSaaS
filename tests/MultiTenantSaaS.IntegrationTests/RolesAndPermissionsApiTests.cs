using System.Net.Http.Headers;
using System.Net.Http.Json;
using MultiTenantSaaS.Domain.Enums;
using MultiTenantSaaS.Shared.Constants;
using Xunit;

namespace MultiTenantSaaS.IntegrationTests;

[Collection("TenantIsolation")]
public class RolesAndPermissionsApiTests
{
    private const string Password = "Password1";

    private readonly TenantIsolationFixture _fixture;

    public RolesAndPermissionsApiTests(TenantIsolationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TenantAdmin_CanManageRolesAndPermissions()
    {
        await ApiTestHelper.SetMembershipRoleAsync(
            _fixture.Factory.Services, "admin@acme.com", MembershipRole.TenantAdmin);

        var token = await ApiTestHelper.LoginAsync(
            _fixture.Client, "admin@acme.com", Password, "acme");

        using var permissionsRequest = new HttpRequestMessage(HttpMethod.Get, "/api/permissions");
        permissionsRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        permissionsRequest.Headers.Add(AppConstants.TenantSlugHeader, "acme");

        var permissionsResponse = await _fixture.Client.SendAsync(permissionsRequest);
        permissionsResponse.EnsureSuccessStatusCode();

        var permissions = await permissionsResponse.Content.ReadFromJsonAsync<List<PermissionResponse>>();
        Assert.NotNull(permissions);
        Assert.Contains(permissions, permission => permission.Name == PermissionNames.ProjectsView);

        using var createRoleRequest = new HttpRequestMessage(HttpMethod.Post, "/api/roles");
        createRoleRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        createRoleRequest.Headers.Add(AppConstants.TenantSlugHeader, "acme");
        createRoleRequest.Content = JsonContent.Create(new
        {
            name = "Support",
            description = "Support team"
        });

        var createRoleResponse = await _fixture.Client.SendAsync(createRoleRequest);
        createRoleResponse.EnsureSuccessStatusCode();

        var createdRole = await createRoleResponse.Content.ReadFromJsonAsync<RoleResponse>();
        Assert.NotNull(createdRole);
        Assert.Equal("Support", createdRole.Name);
        Assert.Empty(createdRole.Permissions);

        using var updatePermissionsRequest = new HttpRequestMessage(
            HttpMethod.Put,
            $"/api/roles/{createdRole.Id}/permissions");
        updatePermissionsRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        updatePermissionsRequest.Headers.Add(AppConstants.TenantSlugHeader, "acme");
        updatePermissionsRequest.Content = JsonContent.Create(new
        {
            permissionNames = new[]
            {
                PermissionNames.ProjectsView,
                PermissionNames.TasksView
            }
        });

        var updatePermissionsResponse = await _fixture.Client.SendAsync(updatePermissionsRequest);
        updatePermissionsResponse.EnsureSuccessStatusCode();

        var updatedRole = await updatePermissionsResponse.Content.ReadFromJsonAsync<RoleResponse>();
        Assert.NotNull(updatedRole);
        Assert.Contains(PermissionNames.ProjectsView, updatedRole.Permissions);
        Assert.Contains(PermissionNames.TasksView, updatedRole.Permissions);

        using var rolesRequest = new HttpRequestMessage(HttpMethod.Get, "/api/roles");
        rolesRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        rolesRequest.Headers.Add(AppConstants.TenantSlugHeader, "acme");

        var rolesResponse = await _fixture.Client.SendAsync(rolesRequest);
        rolesResponse.EnsureSuccessStatusCode();

        var roles = await rolesResponse.Content.ReadFromJsonAsync<List<RoleResponse>>();
        Assert.Contains(roles!, role => role.Id == createdRole.Id);
    }

    private sealed record PermissionResponse(Guid Id, string Name, string? Description);

    private sealed record RoleResponse(
        Guid Id,
        string Name,
        string? Description,
        IReadOnlyList<string> Permissions);
}
