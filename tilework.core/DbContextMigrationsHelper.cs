using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Tilework.Core;

public static class DbContextMigrationsHelper
{
    public static void RunDbMigrations(this IServiceProvider root, IServiceCollection services)
    {
        using var scope = root.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var logger = serviceProvider.GetRequiredService<ILogger<DbContext>>();

        var dbContextTypes = services
            .Where(service => service.ServiceType.IsSubclassOf(typeof(DbContext)))
            .Select(service => service.ServiceType);

        foreach (var dbContextType in dbContextTypes)
        {
            var dbContext = (DbContext)serviceProvider.GetRequiredService(dbContextType);
            logger.LogInformation($"Running migrations for context: {dbContextType}");
            dbContext.Database.Migrate();
        }
    }
}
