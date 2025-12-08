using Application.Users;
using Application.Users.Commands.CreateUser;
using Domain.Common;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using WebApi.Abstractions;

namespace WebApi.Endpoints.Users;

public class CreateUserEndpoint : BaseEndpoint<UserRequest, Results<Ok<UserDTO>, NotFound, ProblemDetails>, UserDTO>
{
    private readonly Application.Abstractions.Messaging.ICommandHandler<CreateUserCommand, UserDTO> _createUserCommandHandler;

    public CreateUserEndpoint(Application.Abstractions.Messaging.ICommandHandler<CreateUserCommand, UserDTO> createUserCommandHandler)
    {
        _createUserCommandHandler = createUserCommandHandler;
    }

    public override void Configure()
    {
        Post("/api/users");
        AllowAnonymous();
    }

    public override async Task<Results<Ok<UserDTO>, NotFound, ProblemDetails>> ExecuteAsync(UserRequest request, CancellationToken cancelationToken)
    {
        CreateUserCommand createUserCommand = new(request);
        Result<UserDTO> result = await _createUserCommandHandler.Handle(createUserCommand, cancelationToken);

        if (result.IsFailure)
        {
            return HandleErrors(result);
        }

        return TypedResults.Ok(result.Value);
    }
}

