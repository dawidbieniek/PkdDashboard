using PkdDashboard.Global;

var builder = DistributedApplication.CreateBuilder(args);

var postgre = builder.AddPostgres(ServiceKeys.PostgreSql)
    .WithPgAdmin()
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);
var authDb = postgre.AddDatabase(ServiceKeys.Database);

var migrator = builder.AddProject<Projects.PkdDashboard_Migrator>(ServiceKeys.Migrator)
    .WithReference(authDb).WaitFor(authDb);

builder.AddProject<Projects.PkdDashboard_WebApp>(ServiceKeys.WebApp)
    .WithReference(authDb).WaitFor(authDb)
    .WaitForCompletion(migrator);

builder.Build().Run();
