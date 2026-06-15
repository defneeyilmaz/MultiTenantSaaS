using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MultiTenantSaaS.Domain.Entities;
using MultiTenantSaaS.Domain.Enums;
using MultiTenantSaaS.Infrastructure.Persistence;
using MultiTenantSaaS.Infrastructure.Security;
using MultiTenantSaaS.Shared.Constants;
using Xunit;

namespace MultiTenantSaaS.IntegrationTests;

[Collection("TenantIsolation")]
public class InvitationFlowTests
{
    private const string Password = "Password1";
    private const string InvitationToken = "integration-test-invitation-token";

    private readonly TenantIsolationFixture _fixture;

    public InvitationFlowTests(TenantIsolationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TenantAdmin_CanInviteUser()
    {
        await ApiTestHelper.SetMembershipRoleAsync(
            _fixture.Factory.Services, "admin@acme.com", MembershipRole.TenantAdmin);

        var token = await ApiTestHelper.LoginAsync(
            _fixture.Client, "admin@acme.com", Password, "acme");

        var inviteEmail = $"employee-{Guid.NewGuid():N}@acme.com";

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/users/invite");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add(AppConstants.TenantSlugHeader, "acme");
        request.Content = JsonContent.Create(new { email = inviteEmail, role = "Employee" });

        var response = await _fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task InvitedUser_CanAcceptInvitationAndJoinTenant()
    {
        using (var scope = _fixture.Factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var tenant = await dbContext.Tenants.SingleAsync(t => t.Slug == "acme");
            var inviter = await dbContext.Users.SingleAsync(u => u.Email == "admin@acme.com");

            dbContext.Invitations.Add(new Invitation
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Email = "newhire@acme.com",
                Role = MembershipRole.Employee,
                TokenHash = RefreshTokenHasher.Hash(InvitationToken),
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                InvitedByUserId = inviter.Id
            });

            await dbContext.SaveChangesAsync();
        }

        var response = await _fixture.Client.PostAsJsonAsync("/api/users/accept-invitation", new
        {
            email = "newhire@acme.com",
            token = InvitationToken,
            password = Password,
            fullName = "New Hire"
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AcceptInvitationResponse>();

        Assert.NotNull(result);
        Assert.Equal("acme", result.TenantSlug);
        Assert.Equal("Employee", result.Role);

        var loginResponse = await _fixture.Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "newhire@acme.com",
            password = Password,
            tenantSlug = "acme"
        });

        loginResponse.EnsureSuccessStatusCode();
    }

    private sealed record AcceptInvitationResponse(
        Guid UserId,
        string Email,
        Guid TenantId,
        string TenantSlug,
        string Role);
}
