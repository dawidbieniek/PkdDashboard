using PkdDashboard.Global;

var builder = DistributedApplication.CreateBuilder(args);

var postgre = builder.AddPostgres(ServiceKeys.PostgreSql)
    .WithPgAdmin()
    .WithLifetime(ContainerLifetime.Persistent);
var db = postgre.AddDatabase(ServiceKeys.Database);

builder.AddProject<Projects.PkdDashboard_WebApp>(ServiceKeys.WebApp)
    .WithReference(db).WaitFor(db);

builder.Build().Run();
