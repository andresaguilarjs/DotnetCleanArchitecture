using Application.Abstractions.Messaging;
using Application.Users;
using Application.Users.Commands.UpdateUser;
using Domain.Common;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using WebApi.Abstractions;

namespace WebApi.Endpoints.Users;

public class UpdateUserEndpoint : BaseEndpoint<UserRequest, Results<Ok<UserDTO>, ProblemDetails>, UserDTO>
{
    private readonly IMediator _mediator;

    public UpdateUserEndpoint(IMediator mediator)
    {
        _mediator = mediator;
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
        Result<UserDTO> result = await _mediator.Send<UpdateUserCommand, UserDTO>(updateUserCommand, cancelationToken);

        if (result.IsFailure)
        {
            return HandleErrors(result);
        }

        return TypedResults.Ok(result.Value);
    }
}

