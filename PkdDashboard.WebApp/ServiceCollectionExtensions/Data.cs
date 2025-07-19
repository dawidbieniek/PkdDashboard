using Microsoft.EntityFrameworkCore;

using PkdDashboard.Global;
using PkdDashboard.Shared.Migrations;
using PkdDashboard.WebApp.Data;
using PkdDashboard.WebApp.Data.Abstract;
using PkdDashboard.WebApp.Data.Seeding;
using PkdDashboard.WebApp.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class Data
{
    private const string MigrationsSchema = "schMigrations";

    internal static IServiceCollection AddWebappDataServices(this IServiceCollection services)
    {
        services.AddScoped<PkdService>();
        services.AddScoped<CompanyCountService>();

        return services;
    }

    internal static void AddWebappDatacontexts(this IHostApplicationBuilder builder)
    {
        string connectionString = builder.Configuration.GetConnectionString(ServiceKeys.Database)
            ?? throw new NullReferenceException($"No connection string provided for '{ServiceKeys.Database}'");

        builder.Services.AddDbContextFactory<AuthDbContext>(ConfigureDbContext<AuthDbContext>(connectionString));
        builder.EnrichDbContext<AuthDbContext>();

        builder.Services.AddDbContextFactory<AppDbContext>(ConfigureDbContext<AppDbContext>(connectionString));
        builder.EnrichDbContext<AppDbContext>();
    }

    public static void AddWebAppMigrators(this IHostApplicationBuilder builder)
    {
        string connectionString = builder.Configuration.GetConnectionString(ServiceKeys.Database)
            ?? throw new NullReferenceException($"No connection string provided for '{ServiceKeys.Database}'");

        builder.Services.AddDbContext<AuthDbContext>(ConfigureDbContext<AuthDbContext>(connectionString));
        builder.EnrichDbContext<AuthDbContext>();

        builder.Services.AddTransient<ISeeder<AppDbContext>, AppDbContextSeeding>();
        builder.Services.AddDbContext<AppDbContext>(ConfigureDbContext<AppDbContext>(connectionString));
        builder.EnrichDbContext<AppDbContext>();

        builder.Services.AddTransient<IMigrator, AuthMigrator>();
        builder.Services.AddTransient<IMigrator, AppMigrator>();
    }

    private static Action<IServiceProvider, DbContextOptionsBuilder> ConfigureDbContext<T>(string connectionString) where T : DbContext, IDbContextWithMigrationTable
    {
        return (sp, options) =>
        {
            options.UseNpgsql(connectionString, options =>
            {
                options.MigrationsHistoryTable(T.MigrationsTable, MigrationsSchema);
            });

            var seeder = sp.GetService<ISeeder<T>>();
            if (seeder is not null)
                options.UseAsyncSeeding(seeder.SeedAsync());
        };
    }

    private static void EnrichDbContext<T>(this IHostApplicationBuilder builder) where T : DbContext, IDbContextWithMigrationTable
    {
        builder.EnrichNpgsqlDbContext<T>(options =>
        {
            options.CommandTimeout = 15;
        });
    }
}