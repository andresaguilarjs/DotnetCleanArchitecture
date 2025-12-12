using Application.Abstractions.Messaging;
using Application.Abstractions.PipelineBehaviors;
using Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Reflection;

namespace Application.Mediator;

/// <summary>
/// Custom mediator implementation that dispatches requests to handlers through a pipeline of behaviors.
/// </summary>
public class Mediator(IServiceProvider serviceProvider) : IMediator
{
    private readonly ConcurrentDictionary<(Type, Type), Type> _handlerTypeCache = new();
    private readonly ConcurrentDictionary<Type, MethodInfo> _handleMethodCache = new();
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    
    /// <summary>
    /// Sends a command request that returns a Result.
    /// This method is used to send a command request that does not return a value.
    /// It also chains the behaviors to the request.
    /// The request is dispatched to the handler and the result is returned.
    /// The result is a Result object that indicates success or failure.
    /// </summary>
    /// <typeparam name="TRequest">The type of the command request.</typeparam>
    /// <param name="request">The command request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result indicating success or failure.</returns>
    public async Task<Result> Send<TRequest>(TRequest request, CancellationToken cancellationToken) 
        where TRequest : ICommand
    {
        Type requestType = typeof(TRequest);
        Type handlerType = GetHandlerType(requestType);
        (object handler, MethodInfo handleMethod) = GetHandlerAndMethod(requestType, handlerType);

        List<IPipelineBehavior<TRequest, Result>> behaviors = ServiceProviderServiceExtensions
            .GetServices<IPipelineBehavior<TRequest, Result>>(_serviceProvider)
            .ToList();

        Func<Task<Result>> pipeline = GetHandlerInvocationForResultResponseType<Result>(handler, handleMethod, request, cancellationToken);

        foreach (IPipelineBehavior<TRequest, Result> behavior in Enumerable.Reverse(behaviors))
        {
            if (behavior == null) continue;

            IPipelineBehavior<TRequest, Result> currentBehavior = behavior;
            Func<Task<Result>> next = pipeline;

            pipeline = async () =>
            {
                return await currentBehavior.HandleAsync(request, cancellationToken, next);
            };
        }

        return await pipeline();
    }

    /// <summary>
    /// Sends a command or query request that returns a Result with a value.
    /// This method is used to send a command or query request that returns a value.
    /// It also chains the behaviors to the request.
    /// The request is dispatched to the handler and the result is returned.
    /// The result is a Result object with a value or errors
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response value.</typeparam>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result containing the response value or errors.</returns>
    public async Task<Result<TResponse>> Send<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
        where TRequest : IBaseRequest
    {
        Type requestType = typeof(TRequest);
        Type responseType = typeof(TResponse);
        Type handlerType = GetHandlerType(requestType, responseType);

        (object handler, MethodInfo handleMethod) = GetHandlerAndMethod(requestType, handlerType);

        List<IPipelineBehavior<TRequest, Result<TResponse>>> behaviors = ServiceProviderServiceExtensions
            .GetServices<IPipelineBehavior<TRequest, Result<TResponse>>>(_serviceProvider).ToList();

        Func<Task<Result<TResponse>>> pipeline = GetHandlerInvocationForResultResponseType<Result<TResponse>>(handler, handleMethod, request, cancellationToken);

        foreach (IPipelineBehavior<TRequest, Result<TResponse>> behavior in Enumerable.Reverse(behaviors))
        {
            if (behavior == null) continue;
            
            IPipelineBehavior<TRequest, Result<TResponse>> currentBehavior = behavior;
            Func<Task<Result<TResponse>>> next = pipeline;

            pipeline = async () =>
            {
                return await currentBehavior.HandleAsync(request, cancellationToken, next);
            };
        }

        return await pipeline();
    }

    /// <summary>
    /// Get the handler invocation for a request
    /// This may be a result with a value or a result without a value.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response value.</typeparam>
    /// <param name="handler">The handler instance.</param>
    /// <param name="handleMethod">The handle method of the handler.</param>
    /// <param name="request">The request instance.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A function that returns the response value.</returns>
    private Func<Task<TResponse>> GetHandlerInvocationForResultResponseType<TResponse>(object handler, MethodInfo handleMethod, object request, CancellationToken cancellationToken)
    {
        return async () =>
        {
            try
            {
                object? invokeResult = handleMethod.Invoke(handler, new object[] { request, cancellationToken });
                
                if (invokeResult == null)
                {
                    throw new InvalidOperationException($"Handler method for {request.GetType().Name} returned null instead of a Task.");
                }

                if (invokeResult is not Task task)
                {
                    throw new InvalidOperationException($"Handler method for {request.GetType().Name} did not return a Task. Returned type: {invokeResult.GetType().Name}");
                }

                await task;

                cancellationToken.ThrowIfCancellationRequested();
                PropertyInfo? resultProperty = task.GetType().GetProperty("Result");
                
                if (resultProperty == null)
                {
                    throw new InvalidOperationException($"Task type {task.GetType().Name} does not have a Result property. Handler may have returned a non-generic Task.");
                }

                object? value = resultProperty.GetValue(task);
                if (value == null)
                {
                    throw new InvalidOperationException($"Handler for {request.GetType().Name} returned null result.");
                }

                return (TResponse)value;
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                throw ex.InnerException;
            }
        };
    }

    /// <summary>
    /// Get the handler type for a request that does not return a value.
    /// </summary>
    /// <param name="requestType">The type of the request.</param>
    /// <returns>The handler type.</returns>
    private Type GetHandlerType(Type requestType)
    {
        return _handlerTypeCache.GetOrAdd((requestType, typeof(Result)), _ =>
        {
            return typeof(ICommandHandler<>).MakeGenericType(requestType);
        });
    }

    /// <summary>
    /// Get the handler type for a request that returns a Result with a value.
    /// </summary>
    /// <param name="requestType">The type of the request.</param>
    /// <param name="responseType">The type of the response value.</param>
    /// <returns>The handler type.</returns>
    private Type GetHandlerType(Type requestType, Type responseType)
    {
        return _handlerTypeCache.GetOrAdd((requestType, responseType), _ =>
        {
            Type? commandInterface = requestType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));

            if (commandInterface != null)
            {
                return typeof(ICommandHandler<,>).MakeGenericType(requestType, responseType);
            }

            return typeof(IQueryHandler<,>).MakeGenericType(requestType, responseType);
        });
    }

    /// <summary>
    /// Get the handler and handle method from the service provider.
    /// </summary>
    /// <param name="requestType">The type of the request.</param>
    /// <param name="handlerType">The type of the handler.</param>
    /// <returns>A tuple containing the handler and handle method.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the handler does not have a Handle method.</exception>
    private (object, MethodInfo) GetHandlerAndMethod(Type requestType, Type handlerType)
    {
        object handler = _serviceProvider.GetRequiredService(handlerType);
        MethodInfo handleMethod = _handleMethodCache.GetOrAdd(handlerType, _ =>
        {
            MethodInfo? method = handlerType.GetMethod(
                "Handle",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { requestType, typeof(CancellationToken) },
                null
            );

            if (method == null)
            {
                throw new InvalidOperationException(
                    $"Handler {handlerType.Name} for {requestType.Name} does not have a Handle method with signature Handle({requestType.Name}, CancellationToken)."
                );
            }

            return method;
        });

        return (handler, handleMethod);
    }
}