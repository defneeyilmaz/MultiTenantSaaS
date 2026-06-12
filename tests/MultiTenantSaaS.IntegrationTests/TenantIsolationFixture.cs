using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MultiTenantSaaS.IntegrationTests;

public class TenantIsolationFixture : IAsyncLifetime
{
    private const string Password = "Password1";

    public IntegrationTestWebApplicationFactory Factory { get; } = new();

    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await Factory.InitializeDatabaseAsync();

        Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        await ApiTestHelper.SignupCompanyAsync(Client, "Acme Corp", "acme", "admin@acme.com", Password);
        await ApiTestHelper.SignupCompanyAsync(Client, "Globex Corp", "globex", "admin@globex.com", Password);

        await Factory.ConfirmEmailAsync("admin@acme.com");
        await Factory.ConfirmEmailAsync("admin@globex.com");
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

[CollectionDefinition("TenantIsolation")]
public class TenantIsolationCollection : ICollectionFixture<TenantIsolationFixture>;
