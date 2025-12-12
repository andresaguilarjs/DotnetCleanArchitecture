using Application.Abstractions.Messaging;
using Application.Users;
using Application.Users.Queries.ReadList;
using Domain.Common;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using WebApi.Abstractions;

namespace WebApi.Endpoints.Users;

public class GetUsersEndpoint : BaseEndpoint<EmptyRequest, Results<Ok<IList<UserDTO>>, NotFound, ProblemDetails>, IList<UserDTO>>
{
    private readonly IMediator _mediator;

    public GetUsersEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/users");
        AllowAnonymous();
    }

    public override async Task<Results<Ok<IList<UserDTO>>, NotFound, ProblemDetails>> ExecuteAsync(EmptyRequest request, CancellationToken cancelationToken)
    {
        ReadUserListQuery readUserListQuery = new();
        Result<IList<UserDTO>> users = await _mediator.Send<ReadUserListQuery, IList<UserDTO>>(readUserListQuery, cancelationToken);

        if (users.IsFailure)
        {
            return HandleErrors(users);
        }

        return TypedResults.Ok(users.Value);
    }
}