using Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebApi.Abstractions;

[ApiController]
public abstract class ApiController : ControllerBase
{
    /// <summary>
    /// Handles the result of a request that does not return a value
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    protected IActionResult HandleResult(Result result)
    {
        if (result.IsFailure)
        {
            return HandleErrorResult(result.Errors);
        }

        return NoContent();
    }

    /// <summary>
    /// Handles the result of a request
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="result"></param>
    /// <returns></returns>
    protected IActionResult HandleResult<T>(Result<T> result, bool isCreation = false, string actionName = "Get")
    {
        if (result.IsFailure)
        {
            return HandleErrorResult(result.Errors);
        }

        if (isCreation)
        {
            return CreatedAtAction(actionName, new { id = result.Value }, result.Value);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Handles the result of a request that is a failure
    /// It can handle different types of errors that are:
    /// - BadRequest
    /// - Unauthorized
    /// - Forbidden
    /// - NotFound
    /// - Default is BadRequest
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    private IActionResult HandleErrorResult(IList<Error> errors)
    {
        if (errors.Any(error => error.Code == ErrorCodes.BadRequest))
        {
            return BadRequest(errors);
        }

        if (errors.Any(error => error.Code == ErrorCodes.Unauthorized))
        {
            return Unauthorized(errors);
        }

        if (errors.Any(error => error.Code == ErrorCodes.Forbidden))
        {
            ModelStateDictionary validationProblemDetails = new ModelStateDictionary();
            foreach (var error in errors)
            {
                validationProblemDetails.AddModelError(error.Code.ToString(), error.Description);
            }

            return ValidationProblem(validationProblemDetails);
        }

        if (errors.Any(error => error.Code == ErrorCodes.NotFound))
        {
            return NotFound(errors);
        }

        return BadRequest(errors);
    }
}
