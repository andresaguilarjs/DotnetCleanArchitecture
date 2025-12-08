using Application.Users;
using Application.Users.Commands.DeleteUser;
using Domain.Common;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using WebApi.Abstractions;

namespace WebApi.Endpoints.Users;

public class DeleteUserEndpoint : BaseEndpoint<EmptyRequest, Results<NoContent, ProblemDetails>, UserDTO>
{
    private readonly Application.Abstractions.Messaging.ICommandHandler<DeleteUserCommand> _deleteUserCommandHandler;

    public DeleteUserEndpoint(Application.Abstractions.Messaging.ICommandHandler<DeleteUserCommand> deleteUserCommandHandler)
    {
        _deleteUserCommandHandler = deleteUserCommandHandler;
    }

    public override void Configure()
    {
        Delete("/api/users/{id}");
        AllowAnonymous();
    }

    public override async Task<Results<NoContent, ProblemDetails>> ExecuteAsync(EmptyRequest request, CancellationToken cancelationToken)
    {
        string? id = Route<string>("id");

        if (id is null || string.IsNullOrEmpty(id) || !Guid.TryParse(id, out Guid userId))
        {
            return GetNotFoundResponse();
        }

        DeleteUserCommand deleteUserCommand = new(userId);
        Result result = await _deleteUserCommandHandler.Handle(deleteUserCommand, cancelationToken);

        if (result.IsFailure)
        {
            return HandleErrors(result);
        }

        return TypedResults.NoContent();
    }
}

