using Microsoft.EntityFrameworkCore;

using PkdDashboard.Global;
using PkdDashboard.Shared.Migrations;
using PkdDashboard.WebApp.Data;
using PkdDashboard.WebApp.Data.Abstract;
using PkdDashboard.WebApp.Data.Seeding;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionsExtensions
{
    private const string MigrationsSchema = "schMigrations";

    internal static void AddWebappDataServices(this IHostApplicationBuilder builder)
    {
        builder.AddDatabase<AuthDbContext>();
        builder.Services.AddTransient<ISeeder<AppDbContext>, AppDbContextSeeding>();
        builder.AddDatabase<AppDbContext>();
    }

    public static void AddWebAppMigrators(this IHostApplicationBuilder builder)
    {
        builder.AddDatabase<AuthDbContext>();
        builder.Services.AddTransient<ISeeder<AppDbContext>, AppDbContextSeeding>();
        builder.AddDatabase<AppDbContext>();

        builder.Services.AddTransient<IMigrator, AuthMigrator>();
        builder.Services.AddTransient<IMigrator, AppMigrator>();
    }

    private static void AddDatabase<T>(this IHostApplicationBuilder builder) where T : DbContext, IDbContextWithMigrationTable
    {
        string connectionString = builder.Configuration.GetConnectionString(ServiceKeys.Database)
            ?? throw new ArgumentNullException($"No connection string provided for '{ServiceKeys.Database}'");

        builder.Services.AddDbContext<T>((sp, options) =>
        {
            options.UseNpgsql(connectionString, options =>
            {
                options.MigrationsHistoryTable(T.MigrationsTable, MigrationsSchema);
            });

            var seeder = sp.GetService<ISeeder<T>>();
            if (seeder is not null)
                options.UseAsyncSeeding(seeder.SeedAsync());
        });

        builder.EnrichNpgsqlDbContext<T>(options =>
        {
            options.CommandTimeout = 15;
        });
    }
}