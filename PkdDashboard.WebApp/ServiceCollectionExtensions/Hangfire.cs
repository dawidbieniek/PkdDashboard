using Hangfire;
using Hangfire.PostgreSql;

using Microsoft.EntityFrameworkCore;

using PkdDashboard.Global;
using PkdDashboard.WebApp;
using PkdDashboard.WebApp.Jobs;
using PkdDashboard.WebApp.Jobs.QueryCompanyCounts;
using PkdDashboard.WebApp.ServiceCollectionExtensions.Config;
using PkdDashboard.WebApp.Services;

using System.Net.Http.Headers;

namespace Microsoft.Extensions.DependencyInjection;

internal static class Hangfire
{
    public static void ConfigureHangfire(this IHostApplicationBuilder builder)
    {
        builder.AddHttpClient();

        string connectionString = builder.GetDbConnectionString();

        builder.Services.AddHangfire(options =>
        {
            options.UseSimpleAssemblyNameTypeSerializer();
            options.UseRecommendedSerializerSettings();
            options.UsePostgreSqlStorage(options =>
                options.UseNpgsqlConnection(connectionString)
            );

            options.UseFilter(new AutomaticRetryAttribute { Attempts = 0 });
        });

        builder.Services.AddHangfireServer(options =>
        {
            options.WorkerCount = 1;
        });
        builder.Services.AddSingleton<HangfireAuthorizationFilter>();

        builder.Services.AddJobs();

        builder.Services.AddScoped<HangfireService>();
    }

    public static void UseWebappHangfireDashboard(this WebApplication app)
    {
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = [app.Services.GetRequiredService<HangfireAuthorizationFilter>()],
            DashboardTitle = "PKD Dashboard Jobs",
            IsReadOnlyFunc = (context) => false,
            IgnoreAntiforgeryToken = true,
        });
    }

    private static void AddHttpClient(this IHostApplicationBuilder builder)
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
        services.AddScoped<QueryCompanyCountsJob>();
        services.AddScoped<HttpService>();
        services.AddScoped<DatabaseService>();

        return services;
    }

    private static string GetDbConnectionString(this IHostApplicationBuilder builder)
        => builder.Configuration.GetConnectionString(ServiceKeys.Database)
            ?? throw new NullReferenceException($"No connection string provided for '{ServiceKeys.Database}'");
}