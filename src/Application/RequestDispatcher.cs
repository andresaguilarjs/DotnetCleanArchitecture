using Application.Abstractions.Messaging;
using Application.Abstractions.PipelineBehaviors;
using Domain.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public class RequestDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public RequestDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<Result> DispatchAsync(IBaseRequest command, CancellationToken cancellationToken = default)
    {
        var requestType = GetRequestType(command);

        Type handlerType = typeof(ICommandHandler<>).MakeGenericType(requestType);
        object handler = GetHandler(handlerType);

        List<dynamic> behaviors = GetBehaviors<Result>(requestType);

        Func<Task<Result>> handlerDelegate = () => ((dynamic)handler).Handle((dynamic)command, cancellationToken);
        handlerDelegate = GetHandlerDelegate<Result>(handlerDelegate, behaviors, command, cancellationToken);
        return await handlerDelegate();
    }

    public async Task<Result<TResult>> DispatchAsync<TResult>(IBaseRequest command, CancellationToken cancellationToken = default)
    {
        Type requestType = GetRequestType(command);

        Type handlerType = GetHandlerType<TResult>(requestType);
        object handler = GetHandler(handlerType);

        List<dynamic> behaviors = GetBehaviors<TResult>(requestType);

        Func<Task<Result<TResult>>> handlerDelegate = () => ((dynamic)handler).Handle((dynamic)command, cancellationToken);
        handlerDelegate = GetHandlerDelegate<Result<TResult>>(handlerDelegate, behaviors, command, cancellationToken);
        return await handlerDelegate();
    }

    private List<dynamic> GetBehaviors<TResult>(Type requestType)
    {
        return _serviceProvider
            .GetServices(typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResult)))
            .Cast<dynamic>()
            .ToList();
    }

    private Func<Task<TResult>> GetHandlerDelegate<TResult>(
        Func<Task<TResult>> handlerDelegate,
        List<dynamic> behaviors,
        IBaseRequest command,
        CancellationToken cancellationToken
        )

    {
        foreach (var behavior in behaviors.AsEnumerable().Reverse())
        {
            var next = handlerDelegate;
            handlerDelegate = () => behavior.HandleAsync((dynamic)command, cancellationToken, next);
        }

        return handlerDelegate;
    }

    private object GetHandler(Type handlerType)
    {
        try {
            return _serviceProvider.GetRequiredService(handlerType);
        }
        catch (InvalidOperationException) {
               throw new InvalidOperationException($"Handler {handlerType.Name} not found. Ensure it's registered in the DI container.");
        }
    }

    private Type GetHandlerType<TResult>(Type requestType)
    {
        if (typeof(IBaseCommand).IsAssignableFrom(requestType))
        {
            return typeof(ICommandHandler<,>).MakeGenericType(requestType, typeof(TResult));
        }

        if (typeof(IBaseQuery).IsAssignableFrom(requestType))
        {
            return typeof(IQueryHandler<,>).MakeGenericType(requestType, typeof(TResult));
        }

        throw new ArgumentException($"Type {requestType.Name} must implement either IBaseCommand or IBaseQuery");
    }

    private Type GetRequestType(IBaseRequest command)
    {
        if (command == null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        return command.GetType();
    }
}