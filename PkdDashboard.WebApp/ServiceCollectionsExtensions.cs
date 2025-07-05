using Microsoft.EntityFrameworkCore;

using PkdDashboard.Global;
using PkdDashboard.Shared.Migrations;
using PkdDashboard.WebApp.Data;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionsExtensions
{
    private const string MigrationsSchema = "schMigrations";

    internal static void AddWebappDataServices(this IHostApplicationBuilder builder)
    {
        builder.AddDatabase();
    }

    public static void AddAuthMigrator(this IHostApplicationBuilder builder)
    {
        builder.AddDatabase();

        builder.Services.AddTransient<IMigrator, AuthMigrator>();
    }

    private static void AddDatabase(this IHostApplicationBuilder builder)
    {
        string connectionString = builder.Configuration.GetConnectionString(ServiceKeys.AuthDatabase)
            ?? throw new ArgumentNullException($"No connection string provided for {ServiceKeys.AuthDatabase}");

        builder.Services.AddDbContext<AuthDbContext>(options =>
        options.UseNpgsql(connectionString, options =>
        {
            options.MigrationsHistoryTable(AuthDbContext.MigrationsTable, MigrationsSchema);
        }));

        builder.EnrichNpgsqlDbContext<AuthDbContext>(options =>
        {
            options.CommandTimeout = 15;
        });
    }
}