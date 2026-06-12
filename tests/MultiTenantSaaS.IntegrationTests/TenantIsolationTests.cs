using System.Net;
using MultiTenantSaaS.Shared.Constants;
using Xunit;

namespace MultiTenantSaaS.IntegrationTests;

[Collection("TenantIsolation")]
public class TenantIsolationTests
{
    private const string Password = "Password1";

    private readonly TenantIsolationFixture _fixture;

    public TenantIsolationTests(TenantIsolationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AuthenticatedUser_WithMismatchedTenantHeader_ReturnsForbidden()
    {
        var acmeToken = await ApiTestHelper.LoginAsync(
            _fixture.Client, "admin@acme.com", Password, "acme");
        await ApiTestHelper.CreateProjectAsync(_fixture.Client, acmeToken, "acme", "Acme Project");

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/projects");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", acmeToken);
        request.Headers.Add(AppConstants.TenantSlugHeader, "globex");

        var response = await _fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task TenantUsers_OnlySeeProjectsFromTheirOwnTenant()
    {
        var acmeToken = await ApiTestHelper.LoginAsync(
            _fixture.Client, "admin@acme.com", Password, "acme");
        var globexToken = await ApiTestHelper.LoginAsync(
            _fixture.Client, "admin@globex.com", Password, "globex");

        var acmeProject = await ApiTestHelper.CreateProjectAsync(
            _fixture.Client, acmeToken, "acme", "Acme Project");
        var globexProject = await ApiTestHelper.CreateProjectAsync(
            _fixture.Client, globexToken, "globex", "Globex Project");

        var acmeProjects = await ApiTestHelper.ListProjectsAsync(_fixture.Client, acmeToken, "acme");
        var globexProjects = await ApiTestHelper.ListProjectsAsync(_fixture.Client, globexToken, "globex");

        Assert.Contains(acmeProjects, project => project.Id == acmeProject.Id);
        Assert.DoesNotContain(acmeProjects, project => project.Id == globexProject.Id);

        Assert.Contains(globexProjects, project => project.Id == globexProject.Id);
        Assert.DoesNotContain(globexProjects, project => project.Id == acmeProject.Id);
    }
}
