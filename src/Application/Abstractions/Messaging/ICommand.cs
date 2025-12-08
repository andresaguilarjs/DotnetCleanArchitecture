using Domain.Common;

namespace Application.Abstractions.Messaging;

/// <summary>
/// Marker interface for a command request. Commands represent operations that change state.
/// </summary>
public interface IBaseCommand : IBaseRequest;

/// <summary>
/// Marker interface for a command request that does not return a value.
/// </summary>
public interface ICommand : ICommand<Result>;

/// <summary>
/// Marker interface for a command request that returns a value.
/// </summary>
/// <typeparam name="TResult"></typeparam>
public interface ICommand<TResult> : IBaseCommand;
