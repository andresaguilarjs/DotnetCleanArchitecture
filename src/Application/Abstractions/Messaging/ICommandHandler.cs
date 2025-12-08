using Domain.Common;

namespace Application.Abstractions.Messaging;

/// <summary>
/// Represents a handler for a command. This handler processes commands that do not return a value.
/// Result is used to indicate success or failure of the command execution.
/// </summary>
/// <typeparam name="TCommand">Type of command to use</typeparam>
public interface ICommandHandler<TCommand> where TCommand : ICommand
{
    Task<Result> Handle(TCommand request, CancellationToken cancellationToken);
}

/// <summary>
/// Represents a handler for a command. This handler processes commands that return a value of type TResult.
/// Result is used to encapsulate the returned value along with success or failure information.
/// </summary>
/// <typeparam name="TCommand">Type of command to use</typeparam>
/// <typeparam name="TResult">Type of data in the response</typeparam>
public interface ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>
{
    Task<Result<TResult>> Handle(TCommand request, CancellationToken cancellationToken);
}