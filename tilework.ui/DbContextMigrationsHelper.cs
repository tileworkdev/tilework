using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

public static class DbContextMigrationsHelper
{
    public static void RunDbMigrations(this IApplicationBuilder app, IServiceCollection services)
    {
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var serviceProvider = scope.ServiceProvider;
            var logger = serviceProvider.GetRequiredService<ILogger<IApplicationBuilder>>();

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
}
