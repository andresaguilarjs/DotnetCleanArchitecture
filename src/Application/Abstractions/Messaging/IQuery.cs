namespace Application.Abstractions.Messaging;

public interface IBaseQuery : IBaseRequest;

public interface IQuery<TResponse> : IBaseQuery;
