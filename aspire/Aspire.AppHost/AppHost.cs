var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.WebApi>("webapi")
    .WithHttpHealthCheck("/health");

builder.Build().Run();
