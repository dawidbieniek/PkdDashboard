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

var builder = WebApplication.CreateBuilder(args);

var proxyGatewayIpString = builder.Configuration[EnvironmentParamsKeys.ProxyGatewayIpKey]
    ?? throw new NullReferenceException("No proxy gateway IP - check your configuration");
if(!IPAddress.TryParse(proxyGatewayIpString, out var proxyGatewayIp))
    throw new FormatException($"Invalid IP address format for {EnvironmentParamsKeys.ProxyGatewayIpKey}: {proxyGatewayIpString}");

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
    options.KnownProxies.Add(proxyGatewayIp);
});

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

app.MapDefaultEndpoints();

// Seed roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await SeedRolesAsync(roleManager);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

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