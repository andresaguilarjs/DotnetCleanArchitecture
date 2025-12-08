using Domain.Common;
using FastEndpoints;

namespace WebApi.Abstractions;

public abstract class BaseEndpoint<TRequest, TResponse, TResult> : Endpoint<TRequest, TResponse> where TRequest : notnull
{
    protected ProblemDetails HandleErrors(Result<TResult> result)
    {
        return HandleErrors(result.Errors);
    }

    protected ProblemDetails HandleErrors(Result result)
    {
        return HandleErrors(result.Errors);
    }

    protected ProblemDetails GetNotFoundResponse()
    {
        AddError(ErrorCode.NotFound.ToString(), "NotFound");
        return new ProblemDetails(ValidationFailures)
        {
            Status = (int)ErrorCode.NotFound
        };
    }

    private ProblemDetails HandleErrors(IList<Error> errors)
    {
        ErrorCode errorCode = GetErrorCode(errors);
        foreach (var error in errors)
        {
            AddError(error.Code.ToString(), error.Description);
        }

        return new ProblemDetails(ValidationFailures)
        {
            Status = (int)errorCode
        };
    }

    private ErrorCode GetErrorCode(IList<Error> errors)
    {
        var firstError = errors.FirstOrDefault();
        return firstError?.Code ?? ErrorCode.InternalServerError;
    }

}