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
        Result<IEnumerable<UserDTO>> result = await Sender.Send(new ReadUserListQuery());
        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var readUserQuery = new ReadUserQuery(id);
        Result<UserDTO> result = await Sender.Send(readUserQuery);

        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Post(UserRequest userRequest)
    {
        var createUserCommand = new CreateUserCommand(
            new Email(userRequest.Email),
            new FirstName(userRequest.FirstName),
            new LastName(userRequest.LastName)
        );

        Result<UserDTO> result = await Sender.Send(createUserCommand);
        if (result.IsSuccess) {
            return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value);
        }

        return BadRequest(result.Error);
    }

    [HttpPut]
    public async Task<IActionResult> Put(UserRequest userRequest)
    {
        if (userRequest.Id is null) {
            return BadRequest("Id is required");
        }

        var updateUserCommand = new UpdateUserCommand(
            userRequest.Id.Value,
            new Email(userRequest.Email),
            new FirstName(userRequest.FirstName),
            new LastName(userRequest.LastName)
        );

        Result<UserDTO> result = await Sender.Send(updateUserCommand);
        return Ok(result.Value);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        Result result = await Sender.Send(new DeleteUserCommand(id));
        return Ok(result);
    }
}
