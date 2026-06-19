using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MultiTenantSaaS.Domain.Enums;
using MultiTenantSaaS.Shared.Constants;
using Xunit;

namespace MultiTenantSaaS.IntegrationTests;

[Collection("TenantIsolation")]
public class AuditLoggingTests
{
    private const string Password = "Password1";

    private readonly TenantIsolationFixture _fixture;

    public AuditLoggingTests(TenantIsolationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TenantAdmin_CanViewAuditLogsAfterLogin()
    {
        await ApiTestHelper.SetMembershipRoleAsync(
            _fixture.Factory.Services, "admin@acme.com", MembershipRole.TenantAdmin);

        var token = await ApiTestHelper.LoginAsync(
            _fixture.Client, "admin@acme.com", Password, "acme");

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/audit-logs");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add(AppConstants.TenantSlugHeader, "acme");

        var response = await _fixture.Client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var logs = await response.Content.ReadFromJsonAsync<List<AuditLogResponse>>();
        Assert.NotNull(logs);
        Assert.Contains(logs, log => log.Action == AuditActions.AuthLogin);
    }

    [Fact]
    public async Task Employee_CannotViewAuditLogs()
    {
        await ApiTestHelper.SetMembershipRoleAsync(
            _fixture.Factory.Services, "admin@acme.com", MembershipRole.Employee);

        var token = await ApiTestHelper.LoginAsync(
            _fixture.Client, "admin@acme.com", Password, "acme");

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/audit-logs");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add(AppConstants.TenantSlugHeader, "acme");

        var response = await _fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private sealed record AuditLogResponse(
        Guid Id,
        Guid TenantId,
        Guid? ActorUserId,
        string? ActorEmail,
        string Action,
        string? EntityType,
        Guid? EntityId,
        string? Details,
        string? IpAddress,
        DateTimeOffset CreatedAt);
}
