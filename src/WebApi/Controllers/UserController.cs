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
    public UserController(
        ICommandHandler<DeleteUserCommand> deleteUserCommandHandler,
        IQueryHandler<ReadUserListQuery, IList<UserDTO>> userListQueryHandler,
        IQueryHandler<ReadUserQuery, UserDTO> readUserQueryHandler,
        ICommandHandler<CreateUserCommand, UserDTO> createUserCommandHandler,
        ICommandHandler<UpdateUserCommand, UserDTO> updateUserCommandHandler
        )
    {
        _deleteUserCommandHandler = deleteUserCommandHandler;
        _userListQueryHandler = userListQueryHandler;
        _readUserQueryHandler = readUserQueryHandler;
        _createUserCommandHandler = createUserCommandHandler;
        _updateUserCommandHandler = updateUserCommandHandler;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        Console.WriteLine("Get");
        Result<IList<UserDTO>> result = await _userListQueryHandler.Handle(new ReadUserListQuery(), CancellationToken.None);
        Console.WriteLine(result.IsSuccess);
        return HandleResult<IList<UserDTO>>(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var readUserQuery = new ReadUserQuery(id);
        Result<UserDTO> result = await _readUserQueryHandler.Handle(readUserQuery, CancellationToken.None);
        return HandleResult<UserDTO>(result);
    }

    [HttpPost]
    public async Task<IActionResult> Post(UserRequest userRequest)
    {
        var createUserCommand = new CreateUserCommand(userRequest);

        Result<UserDTO> result = await _createUserCommandHandler.Handle(createUserCommand, CancellationToken.None);
        return HandleResult<UserDTO>(result, isCreation: true);
    }

    [HttpPut]
    public async Task<IActionResult> Put(UserRequest userRequest)
    {
        if (userRequest.Id is null) {
            return BadRequest("Id is required");
        }

        var updateUserCommand = new UpdateUserCommand(userRequest);

        Result<UserDTO> result = await _updateUserCommandHandler.Handle(updateUserCommand, CancellationToken.None);
        return HandleResult<UserDTO>(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        Result result = await _deleteUserCommandHandler.Handle(new DeleteUserCommand(id), cancellationToken);
        return HandleResult(result);
    }
}
