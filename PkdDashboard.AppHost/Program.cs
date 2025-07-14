using Aspire.Hosting;

using PkdDashboard.AppHost;
using PkdDashboard.Global;

var builder = DistributedApplication.CreateBuilder(args);

var postgre = builder.AddPostgres(ServiceKeys.PostgreSql)
    .WithPgAdmin()
    .WithDataVolume(DockerComposeConfig.Volumes.DatabaseVolumeKey)
    .WithLifetime(ContainerLifetime.Persistent);
var database = postgre.AddDatabase(ServiceKeys.Database);

var migrator = builder.AddProject<Projects.PkdDashboard_Migrator>(ServiceKeys.Migrator)
    .WithReference(database).WaitFor(database);

var dataPolling = builder.AddProject<Projects.PkdDashboard_DataPollingService>(ServiceKeys.DataPollingService)
    .WithReference(database).WaitFor(database)
    .WithEnvironment("ASPNETCORE_URLS", "http://+:8001")
    .WaitForCompletion(migrator);

var webapp = builder.AddProject<Projects.PkdDashboard_WebApp>(ServiceKeys.WebApp)
    .WithReference(database).WaitFor(database)
    .WithReference(dataPolling)
    .WaitForCompletion(migrator)
    .WithEnvironment("ASPNETCORE_URLS", "http://+:8005");

// Configure docker compose generation
builder.AddDockerComposeEnvironment(DockerComposeConfig.ComposeEnvironmentName)
    .ConfigureComposeFile(compose =>
    {
        compose.Networks = new()
        {
            [DockerComposeConfig.Networks.ProxyNetKey] = DockerComposeConfig.Networks.ProxyNet,
            [DockerComposeConfig.Networks.PkdNetKey] = DockerComposeConfig.Networks.PkdNet,
        };
    });

var bizGovApiKeyParam = builder.AddParameter(DockerComposeConfig.Params.BizGovApiKey, secret: true);

postgre.PublishAsDockerComposeService((res, ser) =>
{
    ser.Networks = [DockerComposeConfig.Networks.PkdNetKey, DockerComposeConfig.Networks.ProxyNetKey];
    ser.Ports = ["8000:5432"];
});
migrator.PublishAsDockerComposeService((res, ser) =>
{
    ser.Networks = [DockerComposeConfig.Networks.PkdNetKey];
});
dataPolling.PublishAsDockerComposeService((res, ser) =>
{
    ser.Environment[DockerComposeConfig.Params.BizGovApiKey] = bizGovApiKeyParam.AsEnvironmentPlaceholder(res);
    ser.Networks = [DockerComposeConfig.Networks.PkdNetKey];
    ser.Ports = [];
});
webapp.PublishAsDockerComposeService((res, ser) =>
{
    ser.Networks = [DockerComposeConfig.Networks.PkdNetKey, DockerComposeConfig.Networks.ProxyNetKey];
    ser.Ports = ["8006:8005"];
});

builder.Build().Run();