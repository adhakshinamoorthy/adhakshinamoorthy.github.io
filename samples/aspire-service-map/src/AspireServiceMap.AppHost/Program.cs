var builder = DistributedApplication.CreateBuilder(args);

var catalog = builder.AddProject<Projects.AspireServiceMap_Api>("catalog-api")
    .WithHttpEndpoint(name: "http")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.AspireServiceMap_Api>("orders-api")
    .WithReference(catalog)
    .WithHttpEndpoint(name: "http")
    .WithHttpHealthCheck("/health");

builder.Build().Run();
