using CarRentalSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Extensions
{
    public static class MigrationExtensions
    {
        public static async Task MigrateDbContextAsync<TContext>(this IHost host)
            where TContext : DbContext
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<TContext>>();
            var context = services.GetService<TContext>();

            try
            {
                logger.LogInformation($"Migrating database associated with context {typeof(TContext).Name}");
                await context.Database.MigrateAsync();
                logger.LogInformation($"Migrated database associated with context {typeof(TContext).Name}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"An error occurred while migrating the database used on context {typeof(TContext).Name}");
                throw;
            }
        }

        public static IHost MigrateDatabase(this IHost host)
        {
            host.MigrateDbContextAsync<CarRentalDbContext>().Wait();
            return host;
        }

        public static async Task SeedDatabaseAsync(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                logger.LogInformation("Seeding database...");
                await DatabaseSeeder.SeedDatabase(services);
                logger.LogInformation("Seeded database successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database");
                throw;
            }
        }

        public static IHost SeedDatabase(this IHost host)
        {
            host.SeedDatabaseAsync().Wait();
            return host;
        }
    }
}