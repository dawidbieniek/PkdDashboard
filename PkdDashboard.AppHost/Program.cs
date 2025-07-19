using PkdDashboard.AppHost;
using PkdDashboard.Global;

var builder = DistributedApplication.CreateBuilder(args);

var postgre = builder.AddPostgres(ServiceKeys.PostgreSql)
    .WithPgAdmin()
    .WithDataVolume(DockerComposeConfig.Volumes.DatabaseVolumeKey);
var database = postgre.AddDatabase(ServiceKeys.Database);

var migrator = builder.AddProject<Projects.PkdDashboard_Migrator>(ServiceKeys.Migrator)
    .WithReference(database).WaitFor(database);

var webapp = builder.AddProject<Projects.PkdDashboard_WebApp>(ServiceKeys.WebApp)
    .WithReference(database).WaitFor(database)
    .WaitForCompletion(migrator);
#if !DEBUG
    webapp.WithEnvironment("ASPNETCORE_URLS", "http://+:8005");
#endif

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
    ser.Ports = [];
    ser.Networks = [DockerComposeConfig.Networks.PkdNetKey, DockerComposeConfig.Networks.ProxyNetKey];
    ser.Restart = "unless-stopped";
});
migrator.PublishAsDockerComposeService((res, ser) =>
{
    ser.Build = new()
    {
        Context = "..",
        Dockerfile = "docker/Dockerfile",
        Target = "pkd-migrator"
    };
    ser.Ports = [];
    ser.Networks = [DockerComposeConfig.Networks.PkdNetKey];
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
    ser.Ports = [];
    ser.Networks = [DockerComposeConfig.Networks.PkdNetKey, DockerComposeConfig.Networks.ProxyNetKey];
    ser.Restart = "unless-stopped";
    ser.Environment[EnvironmentParamsKeys.BizGovApiKey] = bizGovApiKeyParam.AsEnvironmentPlaceholder(res);
    ser.Environment["TZ"] = "Europe/Warsaw";
});

builder.Build().Run();