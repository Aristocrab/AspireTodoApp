var builder = DistributedApplication.CreateBuilder(args);

var cache = builder
    .AddRedis("cache");

var postgresdb = builder
    .AddPostgres("postgres")
    // .WithDataVolume(isReadOnly: false) // uncomment to enable saving data to docker volume
    .AddDatabase("postgresdb");

var mongo = builder
    .AddMongoDB("mongo")
    // .WithDataVolume(isReadOnly: false) // uncomment to enable saving data to docker volume
    .AddDatabase("mongodb");

var api = builder
    .AddProject<Projects.AspireTodoApp_ApiService>("apiservice")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache).WaitFor(cache)
    .WithReference(postgresdb).WaitFor(postgresdb)
    .WithReference(mongo).WaitFor(mongo);

builder.AddNpmApp("angular", "../aspire-todo-app-frontend")
    .WithReference(api)
    .WaitFor(api)
    .WithHttpEndpoint(env: "PORT", port: 4200)
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();