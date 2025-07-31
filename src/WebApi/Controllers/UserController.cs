using Application;
using Application.Abstractions.Messaging;
using Application.Users;
using Application.Users.Commands.CreateUser;
using Application.Users.Commands.DeleteUser;
using Application.Users.Commands.UpdateUser;
using Application.Users.Queries.ReadList;
using Application.Users.Queries.ReadUser;
using Domain.Common;
using Microsoft.AspNetCore.Mvc;
using WebApi.Abstractions;

namespace WebApi.Controllers;

[Route("api/users")]
public class UserController : ApiController
{
    private readonly ICommandHandler<DeleteUserCommand> _deleteUserCommandHandler;
    private readonly IQueryHandler<ReadUserListQuery, IList<UserDTO>> _userListQueryHandler;
    private readonly IQueryHandler<ReadUserQuery, UserDTO> _readUserQueryHandler;
    private readonly ICommandHandler<CreateUserCommand, UserDTO> _createUserCommandHandler;
    private readonly ICommandHandler<UpdateUserCommand, UserDTO> _updateUserCommandHandler;

    private readonly RequestDispatcher _dispatcher;

    public UserController(
        ICommandHandler<DeleteUserCommand> deleteUserCommandHandler,
        IQueryHandler<ReadUserListQuery, IList<UserDTO>> userListQueryHandler,
        IQueryHandler<ReadUserQuery, UserDTO> readUserQueryHandler,
        ICommandHandler<CreateUserCommand, UserDTO> createUserCommandHandler,
        ICommandHandler<UpdateUserCommand, UserDTO> updateUserCommandHandler,
        RequestDispatcher dispatcher
        )
    {
        _deleteUserCommandHandler = deleteUserCommandHandler;
        _userListQueryHandler = userListQueryHandler;
        _readUserQueryHandler = readUserQueryHandler;
        _createUserCommandHandler = createUserCommandHandler;
        _updateUserCommandHandler = updateUserCommandHandler;
        _dispatcher = dispatcher;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        ReadUserListQuery readUserListQuery = new();
        Result<IList<UserDTO>> result = await _dispatcher.DispatchAsync<IList<UserDTO>>(readUserListQuery);
        return HandleResult<IList<UserDTO>>(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        ReadUserQuery readUserQuery = new (id);
        Result<UserDTO> result = await _dispatcher.DispatchAsync<UserDTO>(readUserQuery);
        return HandleResult<UserDTO>(result);
    }

    [HttpPost]
    public async Task<IActionResult> Post(UserRequest userRequest)
    {
        CreateUserCommand createUserCommand = new CreateUserCommand(userRequest);

        Result<UserDTO> result = await _dispatcher.DispatchAsync<UserDTO>(createUserCommand);
        return HandleResult<UserDTO>(result, isCreation: true);
    }

    [HttpPut]
    public async Task<IActionResult> Put(UserRequest userRequest)
    {
        if (userRequest.Id is null) {
            return BadRequest("Id is required");
        }

        UpdateUserCommand updateUserCommand = new (userRequest);
        Result<UserDTO> result = await _dispatcher.DispatchAsync<UserDTO>(updateUserCommand);  // _updateUserCommandHandler.Handle(updateUserCommand, CancellationToken.None);
        return HandleResult<UserDTO>(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        DeleteUserCommand deleteUserCommand = new (id);
        Result result = await _dispatcher.DispatchAsync(deleteUserCommand, cancellationToken); // _deleteUserCommandHandler.Handle(deleteUserCommand, cancellationToken);
        return HandleResult(result);
    }
}
