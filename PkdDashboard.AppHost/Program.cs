using PkdDashboard.Global;

var builder = DistributedApplication.CreateBuilder(args);

var postgre = builder.AddPostgres(ServiceKeys.PostgreSql)
    .WithPgAdmin()
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);
var database = postgre.AddDatabase(ServiceKeys.Database);

var migrator = builder.AddProject<Projects.PkdDashboard_Migrator>(ServiceKeys.Migrator)
    .WithReference(database).WaitFor(database);

var dataPolling = builder.AddProject<Projects.PkdDashboard_DataPollingService>(ServiceKeys.DataPollingService)
    .WithReference(database).WaitFor(database)
    .WaitForCompletion(migrator);

builder.AddProject<Projects.PkdDashboard_WebApp>(ServiceKeys.WebApp)
    .WithReference(database).WaitFor(database)
    .WithReference(dataPolling)
    .WaitForCompletion(migrator);

builder.Build().Run();
