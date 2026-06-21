using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using MultiTenantSaaS.Application.Contracts.Security;
using MultiTenantSaaS.Infrastructure.Security;
using Xunit;

namespace MultiTenantSaaS.IntegrationTests;

[Collection("TenantIsolation")]
public class RefreshTokenReuseTests
{
    private const string Password = "Password1";

    private readonly TenantIsolationFixture _fixture;

    public RefreshTokenReuseTests(TenantIsolationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ReusedRefreshToken_RevokesAllActiveSessions()
    {
        ResetRateLimits();

        var initialTokens = await ApiTestHelper.LoginWithTokensAsync(
            _fixture.Client, "admin@acme.com", Password, "acme");

        var rotatedResponse = await ApiTestHelper.RefreshTokenAsync(
            _fixture.Client, initialTokens.RefreshToken);
        rotatedResponse.EnsureSuccessStatusCode();

        var rotatedTokens = await rotatedResponse.Content.ReadFromJsonAsync<AuthTokensResponse>();
        Assert.NotNull(rotatedTokens);

        var reuseResponse = await ApiTestHelper.RefreshTokenAsync(
            _fixture.Client, initialTokens.RefreshToken);
        Assert.Equal(HttpStatusCode.Unauthorized, reuseResponse.StatusCode);

        var followUpResponse = await ApiTestHelper.RefreshTokenAsync(
            _fixture.Client, rotatedTokens.RefreshToken);
        Assert.Equal(HttpStatusCode.Unauthorized, followUpResponse.StatusCode);

        ResetRateLimits();
    }

    private void ResetRateLimits()
    {
        var rateLimitService = _fixture.Factory.Services.GetRequiredService<IRateLimitService>();
        if (rateLimitService is InMemoryRateLimitService inMemoryRateLimitService)
        {
            inMemoryRateLimitService.Reset();
        }
    }

    private sealed record AuthTokensResponse(
        string AccessToken,
        DateTimeOffset AccessTokenExpiresAt,
        string RefreshToken,
        DateTimeOffset RefreshTokenExpiresAt,
        Guid TenantId,
        string TenantSlug,
        Guid UserId,
        string Email,
        string Role);
}
