using PkdDashboard.Migrator;
using PkdDashboard.Shared.Configuration;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddWebAppMigrators();

builder.Services.AddHostedService<Worker>();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(Worker.ActivitySourceName));

var host = builder.Build();

LocalizationUtil.SetDefaultCulture();

host.Run();