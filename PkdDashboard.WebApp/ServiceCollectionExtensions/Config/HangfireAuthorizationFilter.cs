using Hangfire.Dashboard;

using PkdDashboard.WebApp.Data.Entities.Auth;

namespace PkdDashboard.WebApp.ServiceCollectionExtensions.Config;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // For development environment - allow all access
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            return true;

        // For production - check for admin role
        var isAuthorized = httpContext.User.Identity?.IsAuthenticated == true &&
                          httpContext.User.IsInRole(UserRoles.Admin);

        return isAuthorized;
    }
}