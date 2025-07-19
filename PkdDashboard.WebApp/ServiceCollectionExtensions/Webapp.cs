using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

using PkdDashboard.WebApp.Components.Account;
using PkdDashboard.WebApp.Data;
using PkdDashboard.WebApp.Data.Entities.Auth;

namespace Microsoft.Extensions.DependencyInjection;

internal static class Webapp
{
    public static IServiceCollection AddWebapp(this IServiceCollection services)
    {
        services.AddRazorComponents(opt => opt.DetailedErrors = true)
            .AddInteractiveServerComponents(opt => opt.DetailedErrors = true);

        services.AddDatabaseDeveloperPageExceptionFilter();
        services.AddSecurityServices();
        return services;
    }

    public static WebApplication UseSecurity(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();

        return app;
    }

    private static IServiceCollection AddSecurityServices(this IServiceCollection services)
    {
        services.AddAntiforgery();

        services.AddCascadingAuthenticationState();
        services.AddScoped<IdentityUserAccessor>();
        services.AddScoped<IdentityRedirectManager>();
        services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
            .AddIdentityCookies();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
        });

        services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();
        return services;
    }
}