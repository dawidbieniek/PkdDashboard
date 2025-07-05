using PkdDashboard.Global;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.PkdDashboard_WebApp>(ServiceKeys.WebApp);

builder.Build().Run();
