var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("cache", 6379);
builder.AddProject<Projects.Dipchik>("dipchik").WithReference(redis);

builder.Build().Run();
