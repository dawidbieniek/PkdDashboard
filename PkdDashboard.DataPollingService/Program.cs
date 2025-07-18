using Hangfire;

using Microsoft.AspNetCore.HttpOverrides;

using PkdDashboard.DataPollingService;
using PkdDashboard.Global;
using PkdDashboard.Shared.Configuration;

using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddPollingServiceDataServices();

builder.ConfigureHangfire();
builder.Services.AddHangfireServer();

builder.AddHttpClients();

builder.Services.Configure<ForwardedHeadersOptions>(opts => {
    opts.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    var proxyUrl = ServiceDiscoveryUtil.GetServiceEndpoint(ServiceKeys.WebApp, "http");
    if (Uri.TryCreate(proxyUrl, UriKind.Absolute, out var uri))
    {
        // Won't add anything in local dev environment. Only works in docker context.
        if (IPAddress.TryParse(uri.Host, out var ip))
            opts.KnownProxies.Add(ip);
    }
});

var app = builder.Build();
app.UseForwardedHeaders();

LocalizationUtil.SetDefaultCulture();

app.UseHangfireDashboard();

app.MapDefaultEndpoints();
app.MapJobEndpoints();

app.Run();