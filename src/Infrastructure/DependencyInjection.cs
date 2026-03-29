using Domain.Entities.User.Interfaces;
using Domain.Interfaces;
using Infrastructure.Database.Common;
using Infrastructure.Database.DBContext;
using Infrastructure.Database.Repositories.Command;
using Infrastructure.Database.Repositories.Query;
using Infrastructure.HealthChecks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                options => options.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
            );
        });

        services.AddScoped<IUserQueryRepository, UserQueryRepository>();
        services.AddScoped<IUserCommandRepository, UserCommandRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHealthChecks()
            .AddCheck<SqlHealthCheck>("Database Health Check");

        services.AddRabbitMq(configuration);

        return services;
    }

    /// <summary>
    /// Registers MassTransit with RabbitMQ, EF outbox (same DB as <see cref="ApplicationDbContext"/>),
    /// and consumers in this assembly. The API publishes domain events through the outbox; consumers process messages from the broker.
    /// </summary>
    public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        string? rabbitmqUri = configuration.GetConnectionString("rabbitmq") ?? throw new InvalidOperationException("Missing ConnectionStrings:rabbitmq");
        services.AddMassTransit(options =>
        {
            options.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
            {
                // Inbox: ignore duplicate deliveries within this window (relevant for consumers on this host).
                o.DuplicateDetectionWindow = TimeSpan.FromSeconds(30);

                o.UseSqlServer()
                    // Bus outbox: after SaveChanges, a background dispatcher sends staged messages to RabbitMQ.
                    .UseBusOutbox();
                // How often the outbox dispatcher polls for pending outbound messages.
                o.QueryDelay = TimeSpan.FromSeconds(1);
            });

            options.AddConsumers(typeof(DependencyInjection).Assembly);
            options.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitmqUri);
                cfg.ConfigureEndpoints(context);
            });
        });
        return services;
    }
}
