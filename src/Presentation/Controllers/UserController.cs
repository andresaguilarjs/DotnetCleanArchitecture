using Application.Users.Commands.CreateUser;
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
        return Ok("ok");
    }

    [HttpPost]
    public async Task<IActionResult> Post()
    {
        var command = new CreateUserCommand(
            new Email("john.doe@mailinator.com"),
            new FirstName("John"),
            new LastName("Doe")
        );

        var result = await Sender.Send(command);
        return Ok(result);
    }
}
