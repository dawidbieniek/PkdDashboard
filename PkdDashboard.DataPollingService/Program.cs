using Hangfire;

using PkdDashboard.DataPollingService;
using PkdDashboard.Shared.Configuration;

var builder = WebApplication.CreateBuilder(args);

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