using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using MultiTenantSaaS.Application.Contracts.Security;
using MultiTenantSaaS.Infrastructure.Security;
using Xunit;

namespace MultiTenantSaaS.IntegrationTests;

[Collection("TenantIsolation")]
public class RateLimitingTests
{
    private const string Password = "Password1";

    private readonly TenantIsolationFixture _fixture;

    public RateLimitingTests(TenantIsolationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Login_ReturnsTooManyRequests_WhenRateLimitExceeded()
    {
        ResetRateLimits();

        HttpStatusCode? lastStatus = null;

        for (var attempt = 0; attempt < 11; attempt++)
        {
            var response = await _fixture.Client.PostAsJsonAsync("/api/auth/login", new
            {
                email = "ratelimit@example.com",
                password = "WrongPassword1",
                tenantSlug = "acme"
            });

            lastStatus = response.StatusCode;
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, lastStatus);
        ResetRateLimits();
    }

    [Fact]
    public async Task Login_LocksAccount_AfterRepeatedFailedAttempts()
    {
        ResetRateLimits();

        await ApiTestHelper.SignupCompanyAsync(
            _fixture.Client, "Lockout Corp", "lockout", "lockout@test.com", Password);
        await _fixture.Factory.ConfirmEmailAsync("lockout@test.com");

        for (var attempt = 0; attempt < 5; attempt++)
        {
            await _fixture.Client.PostAsJsonAsync("/api/auth/login", new
            {
                email = "lockout@test.com",
                password = "WrongPassword1",
                tenantSlug = "lockout"
            });
        }

        var lockedResponse = await _fixture.Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "lockout@test.com",
            password = Password,
            tenantSlug = "lockout"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, lockedResponse.StatusCode);

        var problem = await lockedResponse.Content.ReadFromJsonAsync<ProblemResponse>();
        Assert.Contains("locked", problem!.Detail, StringComparison.OrdinalIgnoreCase);
        ResetRateLimits();
    }

    [Fact]
    public async Task RefreshToken_ReturnsTooManyRequests_WhenRateLimitExceeded()
    {
        ResetRateLimits();

        HttpStatusCode? lastStatus = null;

        for (var attempt = 0; attempt < 4; attempt++)
        {
            var response = await _fixture.Client.PostAsJsonAsync("/api/auth/refresh-token", new
            {
                refreshToken = "invalid-token"
            });

            lastStatus = response.StatusCode;
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, lastStatus);
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

    private sealed record ProblemResponse(string Detail);
}
