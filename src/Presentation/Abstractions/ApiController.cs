using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Abstractions;

[ApiController]
public abstract class ApiController : ControllerBase
{
    protected ISender Sender;

    protected ApiController(ISender sender)
    {
        Sender = sender;
    }
}
