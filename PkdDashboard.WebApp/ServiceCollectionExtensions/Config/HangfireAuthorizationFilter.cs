using Hangfire.Dashboard;

using PkdDashboard.WebApp.Data.Entities.Auth;

namespace PkdDashboard.WebApp.ServiceCollectionExtensions.Config;

public class HangfireAuthorizationFilter(ILogger<HangfireAuthorizationFilter> logger) : IDashboardAuthorizationFilter
{
    private readonly ILogger<HangfireAuthorizationFilter> _logger = logger;

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        _logger.LogInformation("Hangfire Auth Check: IsAuthenticated={IsAuthenticated}, Username={Username}",
            httpContext.User.Identity?.IsAuthenticated,
            httpContext.User.Identity?.Name ?? "none");

        // For development environment - allow all access
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            _logger.LogWarning("Allowing Hangfire dashboard access in Development environment");
            return true;
        }

        // For production - check for admin role
        var isAuthorized = httpContext.User.Identity?.IsAuthenticated == true &&
                          httpContext.User.IsInRole(UserRoles.Admin);

        _logger.LogInformation("Hangfire authorization result: {IsAuthorized}", isAuthorized);
        return isAuthorized;
    }
}