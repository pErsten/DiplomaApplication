var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("cache");
builder.AddProject<Projects.Dipchik>("dipchik").WithReference(redis);

builder.Build().Run();
