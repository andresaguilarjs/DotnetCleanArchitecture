using Domain.Common;

namespace Application.Abstractions.Messaging;
public interface IBaseCommand : IBaseRequest;
public interface ICommand : ICommand<Result>;
public interface ICommand<TResult> : IBaseCommand;
