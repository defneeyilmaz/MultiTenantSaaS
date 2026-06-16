using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MultiTenantSaaS.Domain.Enums;
using MultiTenantSaaS.Infrastructure.Persistence;
using MultiTenantSaaS.Shared.Constants;
using Xunit;

namespace MultiTenantSaaS.IntegrationTests;

[Collection("TenantIsolation")]
public class UserManagementTests
{
    private const string Password = "Password1";

    private readonly TenantIsolationFixture _fixture;

    public UserManagementTests(TenantIsolationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TenantAdmin_CanListAssignRoleAndDisableUser()
    {
        await ApiTestHelper.SetMembershipRoleAsync(
            _fixture.Factory.Services, "admin@acme.com", MembershipRole.TenantAdmin);

        var adminToken = await ApiTestHelper.LoginAsync(
            _fixture.Client, "admin@acme.com", Password, "acme");

        var memberUserId = await ApiTestHelper.AcceptInvitedUserAsync(
            _fixture,
            "acme",
            "member@acme.com",
            MembershipRole.Employee,
            "admin@acme.com");

        using var listRequest = new HttpRequestMessage(HttpMethod.Get, "/api/users");
        listRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        listRequest.Headers.Add(AppConstants.TenantSlugHeader, "acme");

        var listResponse = await _fixture.Client.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();

        var users = await listResponse.Content.ReadFromJsonAsync<List<TenantUserResponse>>();
        var member = users!.Single(user => user.UserId == memberUserId);
        Assert.Equal("Employee", member.Role);
        Assert.True(member.IsActive);

        using var roleRequest = new HttpRequestMessage(HttpMethod.Patch, $"/api/users/{member.UserId}/role");
        roleRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        roleRequest.Headers.Add(AppConstants.TenantSlugHeader, "acme");
        roleRequest.Content = JsonContent.Create(new { role = "Manager" });

        var roleResponse = await _fixture.Client.SendAsync(roleRequest);
        roleResponse.EnsureSuccessStatusCode();

        var updatedUser = await roleResponse.Content.ReadFromJsonAsync<TenantUserResponse>();
        Assert.Equal("Manager", updatedUser!.Role);

        using var disableRequest = new HttpRequestMessage(HttpMethod.Patch, $"/api/users/{member.UserId}/disable");
        disableRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        disableRequest.Headers.Add(AppConstants.TenantSlugHeader, "acme");

        var disableResponse = await _fixture.Client.SendAsync(disableRequest);
        disableResponse.EnsureSuccessStatusCode();

        var disabledUser = await disableResponse.Content.ReadFromJsonAsync<TenantUserResponse>();
        Assert.False(disabledUser!.IsActive);

        var loginResponse = await _fixture.Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "member@acme.com",
            password = Password,
            tenantSlug = "acme"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }

    private sealed record TenantUserResponse(
        Guid UserId,
        string Email,
        string? FullName,
        string Role,
        bool IsActive,
        DateTimeOffset JoinedAt);
}
