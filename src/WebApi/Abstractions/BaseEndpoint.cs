using Domain.Common;
using FastEndpoints;
using FluentValidation.Results;

namespace WebApi.Abstractions;

/// <summary>
/// Base endpoint class for all endpoints.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public abstract class BaseEndpoint<TRequest, TResponse, TResult>
    : Endpoint<TRequest, TResponse> where TRequest : notnull
{
    /// <summary>
    /// Handles a result and returns a problem details object.
    /// </summary>
    /// <param name="result">The result to handle.</param>
    /// <returns>A problem details object.</returns>
    protected ProblemDetails HandleErrors(Result<TResult> result)
    {
        return HandleErrors(result.Errors);
    }

    /// <summary>
    /// Handles a result and returns a problem details object.
    /// </summary>
    /// <param name="result">The result to handle.</param>
    /// <returns>A problem details object.</returns>
    protected ProblemDetails HandleErrors(Result result)
    {
        return HandleErrors(result.Errors);
    }

    /// <summary>
    /// Gets a not found response.
    /// </summary>
    /// <param name="error">The error to convert to a problem details response.</param>
    /// <returns>A problem details object.</returns>
    protected ProblemDetails GetNotFoundResponse(Error error)
    {
        List<Error> errors = new() { error };
        Dictionary<string, int> errorCodeCounts = GetErrorCodeCounts(errors);
        List<ValidationFailure> validationFailures = ConvertErrorsToValidationFailures(errors, errorCodeCounts);
        
        return new ProblemDetails(validationFailures)
        {
            Status = (int)ErrorCode.NotFound
        };
    }

    /// <summary>
    /// Handles a list of errors and returns a problem details object.
    /// </summary>
    /// <param name="errors">The list of errors to handle.</param>
    /// <returns>A problem details object.</returns>
    private ProblemDetails HandleErrors(IList<Error> errors)
    {
        if (errors == null || errors.Count == 0)
        {
            return new ProblemDetails(new List<ValidationFailure>(), (int)ErrorCode.InternalServerError);
        }

        ErrorCode errorCode = GetErrorCode(errors);

        Dictionary<string, int> errorCodeCounts = GetErrorCodeCounts(errors);
        List<ValidationFailure> validationFailures = ConvertErrorsToValidationFailures(errors, errorCodeCounts);

        ProblemDetails problemDetails = new(validationFailures, (int)errorCode);
        return problemDetails;
    }

    /// <summary>
    /// Converts a list of errors to a list of validation failures.
    /// </summary>
    /// <param name="errors">The list of errors to convert to validation failures.</param>
    /// <param name="errorCodeCounts">The count of errors for each error code.</param>
    /// <returns>A list of validation failures.</returns>
    private List<ValidationFailure> ConvertErrorsToValidationFailures(IList<Error> errors, Dictionary<string, int> errorCodeCounts)
    {
        List<ValidationFailure> validationFailures = new(errors.Count);
        Dictionary<string, int> errorCodeIndices = new();

        foreach (Error error in errors)
        {
            string errorCodeString = error.Code.ToString();
            int totalCount = errorCodeCounts[errorCodeString];

            string propertyName = errorCodeString;
            if (totalCount > 1)
            {
                if (!errorCodeIndices.ContainsKey(errorCodeString))
                {
                    errorCodeIndices[errorCodeString] = 0;
                }
                int index = errorCodeIndices[errorCodeString];
                errorCodeIndices[errorCodeString]++;
                propertyName = $"{errorCodeString}[{index}]";
            }

            validationFailures.Add(new ValidationFailure(propertyName, error.Description)
            {
                ErrorCode = errorCodeString
            });
        }

        return validationFailures;
    }

    /// <summary>
    /// Gets the count of errors for each error code.
    /// </summary>
    /// <param name="errors">The list of errors to get the count of errors for each error code.</param>
    /// <returns>A dictionary with the error code as the key and the count of errors as the value.</returns>
    private Dictionary<string, int> GetErrorCodeCounts(IList<Error> errors)
    {
        Dictionary<string, int> errorCodeCounts = new();
        foreach (Error error in errors)
        {
            string errorCodeString = error.Code.ToString();
            errorCodeCounts.TryGetValue(errorCodeString, out int count);
            errorCodeCounts[errorCodeString] = count + 1;
        }

        return errorCodeCounts;
    }

    /// <summary>
    /// Gets the error code from the first error in the list.
    /// </summary>
    /// <param name="errors">The list of errors to get the error code from.</param>
    /// <returns>The error code from the first error in the list or <see cref="ErrorCode.InternalServerError"/> if the list is empty.</returns>
    private ErrorCode GetErrorCode(IList<Error> errors)
    {
        Error? firstError = errors.FirstOrDefault();
        return firstError?.Code ?? ErrorCode.InternalServerError;
    }
}