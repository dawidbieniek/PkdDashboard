using PkdDashboard.AppHost;
using PkdDashboard.Global;

var builder = DistributedApplication.CreateBuilder(args);

var postgre = builder.AddPostgres(ServiceKeys.PostgreSql)
    .WithPgAdmin()
    .WithDataVolume(DockerComposeConfig.Volumes.DatabaseVolumeKey);
var database = postgre.AddDatabase(ServiceKeys.Database);

var migrator = builder.AddProject<Projects.PkdDashboard_Migrator>(ServiceKeys.Migrator)
    .WithReference(database).WaitFor(database);

var dataPolling = builder.AddProject<Projects.PkdDashboard_DataPollingService>(ServiceKeys.DataPollingService)
    .WithReference(database).WaitFor(database)
    .WaitForCompletion(migrator);

var webapp = builder.AddProject<Projects.PkdDashboard_WebApp>(ServiceKeys.WebApp)
    .WithReference(database).WaitFor(database)
    .WithReference(dataPolling)
    .WaitForCompletion(migrator);

// Configure docker compose generation
builder.AddDockerComposeEnvironment(DockerComposeConfig.ComposeEnvironmentName)
    .ConfigureComposeFile(compose =>
    {
        compose.Name = "pkd-dashboard";
        compose.Networks = new()
        {
            [DockerComposeConfig.Networks.ProxyNetKey] = DockerComposeConfig.Networks.ProxyNet,
            [DockerComposeConfig.Networks.PkdNetKey] = DockerComposeConfig.Networks.PkdNet,
        };
    });

var bizGovApiKeyParam = builder.AddParameter(EnvironmentParamsKeys.BizGovApiKey, secret: true);

postgre.PublishAsDockerComposeService((res, ser) =>
{
    ser.Networks = [DockerComposeConfig.Networks.PkdNetKey, DockerComposeConfig.Networks.ProxyNetKey];
    ser.Ports = [];
});
migrator.PublishAsDockerComposeService((res, ser) =>
{
    ser.Build = new()
    {
        Context = "..",
        Dockerfile = "docker/Dockerfile",
        Target = "pkd-migrator"
    };
    ser.Networks = [DockerComposeConfig.Networks.PkdNetKey]; 
    ser.Restart = "unless-stopped";
    ser.Environment["TZ"] = "Europe/Warsaw";
});
dataPolling.PublishAsDockerComposeService((res, ser) =>
{
    ser.Build = new()
    {
        Context = "..",
        Dockerfile = "docker/Dockerfile",
        Target = "pkd-datapollingservice"
    };
    ser.Networks = [DockerComposeConfig.Networks.PkdNetKey];
    ser.Restart = "unless-stopped";
    ser.Environment[EnvironmentParamsKeys.BizGovApiKey] = bizGovApiKeyParam.AsEnvironmentPlaceholder(res);
    ser.Environment["TZ"] = "Europe/Warsaw";
});
webapp.PublishAsDockerComposeService((res, ser) =>
{
    ser.Build = new()
    {
        Context = "..",
        Dockerfile = "docker/Dockerfile",
        Target = "pkd-dashboard"
    };
    ser.Networks = [DockerComposeConfig.Networks.PkdNetKey, DockerComposeConfig.Networks.ProxyNetKey];
    ser.Restart = "unless-stopped";
    ser.Environment["TZ"] = "Europe/Warsaw";
});

builder.Build().Run();