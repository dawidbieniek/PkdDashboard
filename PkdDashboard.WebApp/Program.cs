using Hangfire;

using Microsoft.AspNetCore.Identity;

using MudBlazor.Services;

using PkdDashboard.Shared.Configuration;
using PkdDashboard.WebApp.Components;
using PkdDashboard.WebApp.Data.Entities.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddWebappDatacontexts();
builder.ConfigureHangfire();
builder.Services.AddWebappDataServices();
builder.Services.AddWebapp();
builder.Services.AddMudServices();

var app = builder.Build();

LocalizationUtil.SetDefaultCulture();

app.UseForwardedHeaders();
app.UseExceptionHandler("/Error", createScopeForErrors: true);
app.UseDeveloperExceptionPage();
app.UseRouting();
app.UseSecurity();
app.UseWebappHangfireDashboard();

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