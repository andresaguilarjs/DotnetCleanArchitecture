using Application;
using Application.Users;
using Application.Users.Commands.CreateUser;
using Application.Users.Commands.DeleteUser;
using Application.Users.Commands.UpdateUser;
using Application.Users.Queries.ReadList;
using Application.Users.Queries.ReadUser;
using Domain.Common;
using Domain.Entities.User.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Presentation.Abstractions;

namespace Presentation.Controllers;

[Route("api/users")]
public class UserController : ApiController
{
    public UserController(ISender sender) : base(sender)
    {
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        Result<IList<UserDTO>> result = await Sender.Send(new ReadUserListQuery());
        return HandleResult<IList<UserDTO>>(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var readUserQuery = new ReadUserQuery(id);
        Result<UserDTO> result = await Sender.Send(readUserQuery);
        return HandleResult<UserDTO>(result);
    }

    [HttpPost]
    public async Task<IActionResult> Post(UserRequest userRequest)
    {
        var createUserCommand = new CreateUserCommand(
            Email.Create(userRequest.Email),
            new FirstName(userRequest.FirstName),
            new LastName(userRequest.LastName)
        );

        Result<UserDTO> result = await Sender.Send(createUserCommand);
        return HandleResult<UserDTO>(result, isCreation: true);
    }

    [HttpPut]
    public async Task<IActionResult> Put(UserRequest userRequest)
    {
        if (userRequest.Id is null) {
            return BadRequest("Id is required");
        }

        var updateUserCommand = new UpdateUserCommand(
            userRequest.Id.Value,
            Email.Create(userRequest.Email),
            new FirstName(userRequest.FirstName),
            new LastName(userRequest.LastName)
        );

        Result<UserDTO> result = await Sender.Send(updateUserCommand);
        return HandleResult<UserDTO>(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        Result result = await Sender.Send(new DeleteUserCommand(id));
        return HandleResult(result);
    }
}
