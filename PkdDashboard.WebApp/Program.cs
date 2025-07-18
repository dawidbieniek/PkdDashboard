using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;

using MudBlazor.Services;

using PkdDashboard.Global;
using PkdDashboard.Shared.Configuration;
using PkdDashboard.WebApp.Components;
using PkdDashboard.WebApp.Components.Account;
using PkdDashboard.WebApp.Data;
using PkdDashboard.WebApp.Data.Entities.Auth;

using System.Net;

using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromMemory(
    [
        new RouteConfig
        {
            RouteId = "hangfire",
            ClusterId = "hangfire-cluster",
            Match = new RouteMatch { Path = "/hangfire/{**catch-all}" }
        }
    ],
    [
        new ClusterConfig
        {
            ClusterId = "hangfire-cluster",
            Destinations = new Dictionary<string, DestinationConfig>
            {
                { "hangfire-destination", new DestinationConfig { Address = ServiceDiscoveryUtil.GetServiceEndpoint(ServiceKeys.DataPollingService, "http") } }
            }
        }
    ])
    .AddTransforms(t => t.AddXForwarded());

builder.AddServiceDefaults();
builder.AddWebAppDataServices();
builder.Services.AddWebAppServices();

// Add services to the container.
builder.Services.AddRazorComponents(opt => opt.DetailedErrors = true)
    .AddInteractiveServerComponents(opt => opt.DetailedErrors = true);

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
    .AddIdentityCookies();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddAntiforgery();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddMudServices();

var app = builder.Build();

LocalizationUtil.SetDefaultCulture();

app.UseForwardedHeaders();
app.UseExceptionHandler("/Error", createScopeForErrors: true);
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapReverseProxy();
app.MapDefaultEndpoints();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapAdditionalIdentityEndpoints();

// Seed roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await SeedRolesAsync(roleManager);
}

app.Run();

static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
{
    if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
    {
        await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
    }

    if (!await roleManager.RoleExistsAsync(UserRoles.User))
    {
        await roleManager.CreateAsync(new IdentityRole(UserRoles.User));
    }
}