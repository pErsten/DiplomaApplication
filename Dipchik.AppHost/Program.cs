var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Dipchik>("dipchik");

builder.Build().Run();
