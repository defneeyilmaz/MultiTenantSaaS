using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MultiTenantSaaS.Domain.Enums;
using Xunit;

namespace MultiTenantSaaS.IntegrationTests;

[Collection("TenantIsolation")]
public class PlatformTenantManagementTests
{
    private const string Password = "Password1";

    private readonly TenantIsolationFixture _fixture;

    public PlatformTenantManagementTests(TenantIsolationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task PlatformAdmin_CanManageTenants()
    {
        await ApiTestHelper.SetMembershipRoleAsync(
            _fixture.Factory.Services, "admin@acme.com", MembershipRole.PlatformAdmin);

        var token = await ApiTestHelper.LoginAsync(
            _fixture.Client, "admin@acme.com", Password, "acme");

        using var listRequest = new HttpRequestMessage(HttpMethod.Get, "/api/platform/tenants");
        listRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var listResponse = await _fixture.Client.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();

        var tenants = await listResponse.Content.ReadFromJsonAsync<List<PlatformTenantResponse>>();
        Assert.NotNull(tenants);
        Assert.True(tenants.Count >= 2);

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/platform/tenants");
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        createRequest.Content = JsonContent.Create(new
        {
            name = "Initech",
            slug = "initech",
            domain = "initech.example.com"
        });

        var createResponse = await _fixture.Client.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<PlatformTenantResponse>();
        Assert.NotNull(created);
        Assert.Equal("initech", created.Slug);
        Assert.True(created.IsActive);

        using var disableRequest = new HttpRequestMessage(HttpMethod.Patch, $"/api/platform/tenants/{created.Id}");
        disableRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        disableRequest.Content = JsonContent.Create(new { isActive = false });

        var disableResponse = await _fixture.Client.SendAsync(disableRequest);
        disableResponse.EnsureSuccessStatusCode();

        var disabled = await disableResponse.Content.ReadFromJsonAsync<PlatformTenantResponse>();
        Assert.NotNull(disabled);
        Assert.False(disabled.IsActive);
    }

    [Fact]
    public async Task TenantAdmin_CannotAccessPlatformTenants()
    {
        await ApiTestHelper.SetMembershipRoleAsync(
            _fixture.Factory.Services, "admin@acme.com", MembershipRole.TenantAdmin);

        var token = await ApiTestHelper.LoginAsync(
            _fixture.Client, "admin@acme.com", Password, "acme");

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/platform/tenants");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private sealed record PlatformTenantResponse(
        Guid Id,
        string Name,
        string Slug,
        string? Domain,
        bool IsActive,
        DateTimeOffset CreatedAt);
}
