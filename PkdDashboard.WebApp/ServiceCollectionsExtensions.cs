#pragma warning disable IDE0130 // Namespace does not match folder structure
using PkdDashboard.Global;
using PkdDashboard.WebApp.Data;

namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class ServiceCollectionsExtensions
{
    private const string MigrationsSchema = "schMigrations";

    public static IServiceCollection AddWebappDataServices(this IServiceCollection services)
    {
        services.AddNpgsql<AuthDbContext>(ServiceKeys.Database, options =>
        {
            options.MigrationsHistoryTable(AuthDbContext.MigrationsTable, MigrationsSchema);
        });

        return services;
    }
}
