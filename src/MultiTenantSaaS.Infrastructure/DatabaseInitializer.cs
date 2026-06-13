using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MultiTenantSaaS.Infrastructure.Persistence;
using MultiTenantSaaS.Infrastructure.Persistence.Seed;

namespace MultiTenantSaaS.Infrastructure;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.MigrateAsync();
        await AuthorizationSeedData.SeedAsync(dbContext);
    }
}
