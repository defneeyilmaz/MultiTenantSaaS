using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MultiTenantSaaS.Domain.Enums;
using MultiTenantSaaS.Shared.Constants;
using Xunit;

namespace MultiTenantSaaS.IntegrationTests;

[Collection("TenantIsolation")]
public class PermissionAuthorizationTests
{
    private const string Password = "Password1";

    private readonly TenantIsolationFixture _fixture;

    public PermissionAuthorizationTests(TenantIsolationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TenantAdmin_CannotCreateProject_WithoutPermission()
    {
        await ApiTestHelper.SetMembershipRoleAsync(
            _fixture.Factory.Services, "admin@acme.com", MembershipRole.TenantAdmin);

        var token = await ApiTestHelper.LoginAsync(
            _fixture.Client, "admin@acme.com", Password, "acme");

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/projects");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add(AppConstants.TenantSlugHeader, "acme");
        request.Content = JsonContent.Create(new { name = "Forbidden Project", description = (string?)null });

        var response = await _fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Manager_CanCreateProject_WithPermission()
    {
        await ApiTestHelper.SetMembershipRoleAsync(
            _fixture.Factory.Services, "admin@acme.com", MembershipRole.Manager);

        var token = await ApiTestHelper.LoginAsync(
            _fixture.Client, "admin@acme.com", Password, "acme");

        var project = await ApiTestHelper.CreateProjectAsync(
            _fixture.Client, token, "acme", "Manager Project");

        Assert.Equal("Manager Project", project.Name);
    }
}
