var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("cache", 6379);
var postgres = builder.AddPostgres("sql");
var backend = builder.AddProject<Projects.Backend>("backend")
    .WithReference(redis)
    .WithReference(postgres);
builder.AddProject<Projects.Client>("client")
    .WithReference(backend);
       

builder.Build().Run();
