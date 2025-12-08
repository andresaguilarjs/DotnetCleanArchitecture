using Application.Users;
using Application.Users.Commands.UpdateUser;
using Domain.Common;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using WebApi.Abstractions;

namespace WebApi.Endpoints.Users;

public class UpdateUserEndpoint : BaseEndpoint<UserRequest, Results<Ok<UserDTO>, ProblemDetails>, UserDTO>
{
    private readonly Application.Abstractions.Messaging.ICommandHandler<UpdateUserCommand, UserDTO> _updateUserCommandHandler;

    public UpdateUserEndpoint(Application.Abstractions.Messaging.ICommandHandler<UpdateUserCommand, UserDTO> updateUserCommandHandler)
    {
        _updateUserCommandHandler = updateUserCommandHandler;
    }

    public override void Configure()
    {
        Put("/api/users/{id}");
        AllowAnonymous();
    }

    public override async Task<Results<Ok<UserDTO>, ProblemDetails>> ExecuteAsync(UserRequest request, CancellationToken cancelationToken)
    {
        UserRequest userRequest = new(request.Email, request.FirstName, request.LastName, request.Id);
        UpdateUserCommand updateUserCommand = new(userRequest);
        Result<UserDTO> result = await _updateUserCommandHandler.Handle(updateUserCommand, cancelationToken);

        if (result.IsFailure)
        {
            return HandleErrors(result);
        }

        return TypedResults.Ok(result.Value);
    }
}

