var builder = DistributedApplication.CreateBuilder(args);

var rabbitmq = builder.AddRabbitMQ(
    "rabbitmq",
    userName: GetUserParameter(builder),
    password: GetPasswordParameter(builder),
    port: GetPortNumber())
    .WithManagementPlugin();

builder.AddProject<Projects.WebApi>("webapi")
    .WithReference(rabbitmq)
    .WithHttpHealthCheck("/health");

builder.Build().Run();


int GetPortNumber()
{
    string port = Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? throw new InvalidOperationException("RABBITMQ_PORT environment variable was not found");
    int portNumber;
    bool success = int.TryParse(port, out portNumber);

    if (!success)
    {
        throw new InvalidOperationException("RABBITMQ_PORT is not a valid number.");
    }
    return portNumber;
}

IResourceBuilder<ParameterResource> GetUserParameter(IDistributedApplicationBuilder builder)
{
    string userName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? throw new InvalidOperationException("RABBITMQ_USER environment variable was not found");
    return builder.AddParameter("rabbitmq-user", userName);
}

IResourceBuilder<ParameterResource> GetPasswordParameter(IDistributedApplicationBuilder builder)
{
    string password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? throw new InvalidOperationException("RABBITMQ_PASSWORD environment variable was not found");
    return builder.AddParameter("rabbitmq-password", password, secret: true);
}
