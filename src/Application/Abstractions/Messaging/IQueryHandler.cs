using Application.Abstractions.Messaging;
using Domain.Common;
using MediatR;

namespace Application;

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>> where TQuery : IQuery<TResponse>
{
}
