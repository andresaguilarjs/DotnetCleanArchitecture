using Application.Abstractions.Messaging;
using Application.Users;
using Application.Users.Commands.CreateUser;
using Domain.Common;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using WebApi.Abstractions;

namespace WebApi.Endpoints.Users;

public class CreateUserEndpoint : BaseEndpoint<UserRequest, Results<Ok<UserDTO>, NotFound, ProblemDetails>, UserDTO>
{
    private readonly IMediator _mediator;

    public CreateUserEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/users");
        AllowAnonymous();
    }

    public override async Task<Results<Ok<UserDTO>, NotFound, ProblemDetails>> ExecuteAsync(UserRequest request, CancellationToken cancelationToken)
    {
        CreateUserCommand createUserCommand = new(request);
        Result<UserDTO> result = await _mediator.Send<CreateUserCommand, UserDTO>(createUserCommand, cancelationToken);

        if (result.IsFailure)
        {
            return HandleErrors(result);
        }

        return TypedResults.Ok(result.Value);
    }
}

