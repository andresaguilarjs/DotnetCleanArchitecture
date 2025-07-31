using Application.Abstractions.Messaging;
using Application.Behaviors;
using Application.Middlewares;
using Application.Users;
using Application.Users.Commands.CreateUser;
using Application.Users.Commands.DeleteUser;
using Application.Users.Commands.UpdateUser;
using Application.Users.Queries.ReadList;
using Application.Users.Queries.ReadUser;
using Application.Users.Services;
using Domain.Entities.User.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    /// <summary>
    /// Adds application services to the service collection.
    /// </summary>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehavior<,>));

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<RequestDispatcher>();

        // Commands
        services.AddScoped<ICommandHandler<DeleteUserCommand>, DeleteUserCommandHandler>();
        services.AddScoped<ICommandHandler<CreateUserCommand, UserDTO>, CreateUserCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateUserCommand, UserDTO>, UpdateUserCommandHandler>();

        // Queries
        services.AddScoped<IQueryHandler<ReadUserListQuery, IList<UserDTO>>, ReadUserListQueryHandler>();
        services.AddScoped<IQueryHandler<ReadUserQuery, UserDTO>, ReadUserQueryHandler>();

        services.AddTransient<ExceptionHanddlerMiddleware>();
        return services;
    }
}