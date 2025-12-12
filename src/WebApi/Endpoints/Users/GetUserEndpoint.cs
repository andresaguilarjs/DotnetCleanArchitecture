using Application.Abstractions.Messaging;
using Application.Users;
using Application.Users.Queries.ReadUser;
using Domain.Common;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using WebApi.Abstractions;

namespace WebApi.Endpoints.Users;

public class GetUserEndpoint : BaseEndpoint<EmptyRequest, Results<Ok<UserDTO>, ProblemDetails>, UserDTO>
{
    private readonly IMediator _mediator;

    public GetUserEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/users/{id}");
        AllowAnonymous();
    }

    public override async Task<Results<Ok<UserDTO>, ProblemDetails>> ExecuteAsync(EmptyRequest request, CancellationToken cancelationToken)
    {
        string? id = Route<string>("id");
        if (id is null || string.IsNullOrEmpty(id) || !Guid.TryParse(id, out Guid userId))
        {
            return GetNotFoundResponse();
        }

        ReadUserQuery readUserQuery = new(userId);
        Result<UserDTO> result = await _mediator.Send<ReadUserQuery, UserDTO>(readUserQuery, cancelationToken);

        if (result.IsFailure)
        {
            return HandleErrors(result);
        }

        return TypedResults.Ok(result.Value);
    }
}

