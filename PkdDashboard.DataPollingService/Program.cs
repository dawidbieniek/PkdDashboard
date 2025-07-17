using Hangfire;

using Microsoft.AspNetCore.HttpOverrides;

using PkdDashboard.DataPollingService;
using PkdDashboard.Global;
using PkdDashboard.Shared.Configuration;

using System.Net;

var builder = WebApplication.CreateBuilder(args);

// HACK: Temporarly expose in proxy network
var proxyGatewayIpString = builder.Configuration[EnvironmentParamsKeys.ProxyGatewayIpKey]
    ?? throw new NullReferenceException("No proxy gateway IP - check your configuration");
if (!IPAddress.TryParse(proxyGatewayIpString, out var proxyGatewayIp))
    throw new FormatException($"Invalid IP address format for {EnvironmentParamsKeys.ProxyGatewayIpKey}: {proxyGatewayIpString}");
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
    options.KnownProxies.Add(proxyGatewayIp);
});

builder.AddServiceDefaults();
builder.AddPollingServiceDataServices();

builder.ConfigureHangfire();
builder.Services.AddHangfireServer();

builder.AddHttpClients();

var app = builder.Build();

LocalizationUtil.SetDefaultCulture();

app.MapJobEndpoints();
app.MapDefaultEndpoints();
app.UseHangfireDashboard();

app.Run();