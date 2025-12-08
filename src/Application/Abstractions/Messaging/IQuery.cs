namespace Application.Abstractions.Messaging;

/// <summary>
/// Marker interface for a query request. Queries represent operations that retrieve data without changing state.
/// </summary>
public interface IBaseQuery : IBaseRequest;

/// <summary>
/// General interface for a query request, they return a response of type TResponse.
/// </summary>
public interface IQuery<TResponse> : IBaseQuery;
