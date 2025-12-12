using Domain.Common;

namespace Application.Abstractions.Messaging;

/// <summary>
/// Mediator interface for dispatching requests to handlers through a pipeline of behaviors.
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Sends a command request that returns a Result.
    /// </summary>
    /// <typeparam name="TRequest">The type of the command request.</typeparam>
    /// <param name="request">The command request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result indicating success or failure.</returns>
    Task<Result> Send<TRequest>(TRequest request, CancellationToken cancellationToken) where TRequest : ICommand;

    /// <summary>
    /// Sends a command or query request that returns a Result with a value.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response value.</typeparam>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result containing the response value or errors.</returns>
    Task<Result<TResponse>> Send<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken) 
        where TRequest : IBaseRequest;
}

