using Microsoft.EntityFrameworkCore;

using PkdDashboard.Global;
using PkdDashboard.Shared.Migrations;
using PkdDashboard.WebApp.Data;
using PkdDashboard.WebApp.Data.Abstract;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionsExtensions
{
    private const string MigrationsSchema = "schMigrations";

    internal static void AddWebappDataServices(this IHostApplicationBuilder builder)
    {
        builder.AddDatabase<AuthDbContext>();
        builder.AddDatabase<AppDbContext>();
    }

    public static void AddWebAppMigrators(this IHostApplicationBuilder builder)
    {
        builder.AddDatabase<AuthDbContext>();
        builder.AddDatabase<AppDbContext>();

        builder.Services.AddTransient<IMigrator, AuthMigrator>();
        builder.Services.AddTransient<IMigrator, AppMigrator>();
    }

    private static void AddDatabase<T>(this IHostApplicationBuilder builder) where T : DbContext, IDbContextWithMigrationTable
    {
        string connectionString = builder.Configuration.GetConnectionString(ServiceKeys.Database)
            ?? throw new ArgumentNullException($"No connection string provided for '{ServiceKeys.Database}'");

        builder.Services.AddDbContext<T>(options =>
            options.UseNpgsql(connectionString, options =>
            {
                options.MigrationsHistoryTable(T.MigrationsTable, MigrationsSchema);
            }));

        builder.EnrichNpgsqlDbContext<T>(options =>
        {
            options.CommandTimeout = 15;
        });
    }
}