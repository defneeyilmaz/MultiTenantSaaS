using System.Net.Http.Headers;
using System.Net.Http.Json;
using MultiTenantSaaS.Shared.Constants;

namespace MultiTenantSaaS.IntegrationTests;

internal static class ApiTestHelper
{
    public static async Task SignupCompanyAsync(
        HttpClient client,
        string companyName,
        string slug,
        string email,
        string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/company-signup", new
        {
            companyName,
            adminEmail = email,
            adminPassword = password,
            companySlug = slug,
            adminFullName = $"{companyName} Admin"
        });

        response.EnsureSuccessStatusCode();
    }

    public static async Task<string> LoginAsync(
        HttpClient client,
        string email,
        string password,
        string tenantSlug)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password,
            tenantSlug
        });

        response.EnsureSuccessStatusCode();
        var tokens = await response.Content.ReadFromJsonAsync<AuthTokensResponse>();
        return tokens!.AccessToken;
    }

    public static async Task<ProjectResponse> CreateProjectAsync(
        HttpClient client,
        string accessToken,
        string tenantSlug,
        string name)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/projects");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add(AppConstants.TenantSlugHeader, tenantSlug);
        request.Content = JsonContent.Create(new { name, description = (string?)null });

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ProjectResponse>())!;
    }

    public static async Task<IReadOnlyList<ProjectResponse>> ListProjectsAsync(
        HttpClient client,
        string accessToken,
        string tenantSlug)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/projects");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add(AppConstants.TenantSlugHeader, tenantSlug);

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<List<ProjectResponse>>())!;
    }

    public static async Task<HttpResponseMessage> ListProjectsRawAsync(
        HttpClient client,
        string accessToken,
        string tenantSlug)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/projects");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add(AppConstants.TenantSlugHeader, tenantSlug);

        return await client.SendAsync(request);
    }

    private sealed record AuthTokensResponse(string AccessToken);

    internal sealed record ProjectResponse(Guid Id, string Name, string? Description, DateTimeOffset CreatedAt);
}
