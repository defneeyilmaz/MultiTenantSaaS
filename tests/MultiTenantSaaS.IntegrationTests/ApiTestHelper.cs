using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MultiTenantSaaS.Domain.Entities;
using MultiTenantSaaS.Domain.Enums;
using MultiTenantSaaS.Infrastructure.Persistence;
using MultiTenantSaaS.Infrastructure.Security;
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

    public static async Task SetMembershipRoleAsync(
        IServiceProvider services,
        string email,
        MembershipRole role)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await dbContext.Users.SingleAsync(u => u.Email == normalizedEmail);
        var membership = await dbContext.UserTenantMemberships.SingleAsync(m => m.UserId == user.Id);
        membership.Role = role;
        await dbContext.SaveChangesAsync();
    }

    public static async Task<Guid> AcceptInvitedUserAsync(
        TenantIsolationFixture fixture,
        string tenantSlug,
        string email,
        MembershipRole role,
        string inviterEmail)
    {
        const string invitationToken = "user-management-invitation-token";

        using (var scope = fixture.Factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var tenant = await dbContext.Tenants.SingleAsync(t => t.Slug == tenantSlug);
            var inviter = await dbContext.Users.SingleAsync(u => u.Email == inviterEmail);

            dbContext.Invitations.Add(new Invitation
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Email = email,
                Role = role,
                TokenHash = RefreshTokenHasher.Hash(invitationToken),
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                InvitedByUserId = inviter.Id
            });

            await dbContext.SaveChangesAsync();
        }

        var response = await fixture.Client.PostAsJsonAsync("/api/users/accept-invitation", new
        {
            email,
            token = invitationToken,
            password = "Password1",
            fullName = "Tenant Member"
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AcceptInvitationResponse>();
        return result!.UserId;
    }

    private sealed record AuthTokensResponse(string AccessToken);

    private sealed record AcceptInvitationResponse(Guid UserId);

    internal sealed record ProjectResponse(Guid Id, string Name, string? Description, DateTimeOffset CreatedAt);
}
