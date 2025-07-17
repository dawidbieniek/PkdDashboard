using Hangfire;
using Hangfire.PostgreSql;

using Microsoft.EntityFrameworkCore;

using PkdDashboard.DataPollingService;
using PkdDashboard.DataPollingService.Data;
using PkdDashboard.DataPollingService.Jobs;
using PkdDashboard.DataPollingService.Jobs.QueryCompanyCounts;
using PkdDashboard.Global;

using System.Net.Http.Headers;

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

    public static void AddHttpClients(this IHostApplicationBuilder builder)
    {
        string token = builder.Configuration[EnvironmentParamsKeys.BizGovApiKey]
            ?? throw new NullReferenceException("No JWT for authorization - check your configuration");

        builder.Services.AddHttpClient(HttpClientKeys.BiznesGovKey, options =>
        {
            options.BaseAddress = new("https://dane.biznes.gov.pl/api/ceidg/v3/");
            options.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        });
    }

    private static IServiceCollection AddJobs(this IServiceCollection services)
    {
        services.AddScoped<IQueryCompanyCountsJob, QueryCompanyCountsJob>();
        services.AddScoped<HttpService>();
        services.AddScoped<DatabaseService>();

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