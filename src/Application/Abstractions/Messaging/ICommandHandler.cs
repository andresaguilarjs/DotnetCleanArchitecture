using Domain.Common;

namespace Application.Abstractions.Messaging;

public interface ICommandHandler<TCommand> where TCommand : ICommand
{
    Task<Result> Handle(TCommand request, CancellationToken cancellationToken);
}

public interface ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>
{
    Task<Result<TResult>> Handle(TCommand request, CancellationToken cancellationToken);
}