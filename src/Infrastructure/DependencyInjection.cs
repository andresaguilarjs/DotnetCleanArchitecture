using Domain.Entities.User.Interfaces;
using Domain.Interfaces;
using Infrastructure.Database.Common;
using Infrastructure.Database.DBContext;
using Infrastructure.Database.Repositories.Command;
using Infrastructure.Database.Repositories.Query;
using Infrastructure.HealthChecks;
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
            .AddCheck<SqlHealthCheck>("Sql Health Check");

        return services;
    }
}
