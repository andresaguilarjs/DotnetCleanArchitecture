using Domain.Common;

namespace Application.Abstractions.Messaging;

/// <summary>
/// Represents a handler for a query. This handler processes queries that return a value of type TResponse.
/// </summary>
/// <typeparam name="TQuery"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public interface IQueryHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> Handle(TQuery request, CancellationToken cancellationToken);
}
