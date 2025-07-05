using Microsoft.EntityFrameworkCore;

using PkdDashboard.Global;
using PkdDashboard.Shared.Migrations;
using PkdDashboard.WebApp.Data;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionsExtensions
{
    private const string MigrationsSchema = "schMigrations";

    internal static IServiceCollection AddWebappDataServices(this IServiceCollection services)
    {
        services.AddDatabase(ServiceKeys.AuthDatabase);

        return services;
    }

    public static void AddAuthMigrator(this IHostApplicationBuilder builder)
    {
        string connectionString = builder.Configuration.GetConnectionString(ServiceKeys.AuthDatabase)
            ?? throw new ArgumentNullException($"No connection string provided for {ServiceKeys.AuthDatabase}");

        IServiceCollection services = builder.Services;
        services.AddDatabase(connectionString);

        services.AddTransient<IMigrator, AuthMigrator>();
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddNpgsql<AuthDbContext>(connectionString, options =>
        {
            options.MigrationsHistoryTable(AuthDbContext.MigrationsTable, MigrationsSchema);
        });

        return services;
    }
}