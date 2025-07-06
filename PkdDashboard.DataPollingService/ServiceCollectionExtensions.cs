using Hangfire;
using Hangfire.PostgreSql;

using Microsoft.EntityFrameworkCore;

using PkdDashboard.DataPollingService.Data;
using PkdDashboard.DataPollingService.Jobs;
using PkdDashboard.Global;

namespace Microsoft.Extensions.DependencyInjection;

internal static class ServiceCollectionExtensions
{
    internal static void AddPollingServiceDataServices(this IHostApplicationBuilder builder)
    {
        builder.AddDatabase<PkdDbContext>();
    }

    public static void ConfigureHangfire(this IHostApplicationBuilder builder)
    {
        string connectionString = builder.GetDbConnectionString();

        builder.Services.AddHangfire(options =>
        {
            options.UseSimpleAssemblyNameTypeSerializer();
            options.UseRecommendedSerializerSettings();
            options.UsePostgreSqlStorage(options =>
                options.UseNpgsqlConnection(connectionString)
            );
        });

        builder.Services.AddJobs();
    }

    private static IServiceCollection AddJobs(this IServiceCollection services)
    {
        services.AddScoped<IQueryCompanyCountsJob, QueryCompanyCountsJob>();

        return services;
    }
    private static void AddDatabase<T>(this IHostApplicationBuilder builder) where T : DbContext
    {
        string connectionString = builder.GetDbConnectionString();

        builder.Services.AddDbContext<T>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
        });

        builder.EnrichNpgsqlDbContext<T>(options =>
        {
            options.CommandTimeout = 15;
        });
    }

    private static string GetDbConnectionString(this IHostApplicationBuilder builder)
        => builder.Configuration.GetConnectionString(ServiceKeys.Database)
            ?? throw new NullReferenceException($"No connection string provided for '{ServiceKeys.Database}'");
}