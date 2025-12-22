using Domain.Common;
using FastEndpoints;
using FluentValidation.Results;

namespace WebApi.Common;

/// <summary>
/// Provides functionality to convert application errors and exceptions into standardized problem details suitable for
/// HTTP API responses.
/// </summary>
/// <remarks>This class is intended to facilitate consistent error handling in web applications by mapping domain
/// errors and exceptions to problem details that conform to common API error response formats. It is designed for use
/// in ASP.NET Core applications and is not intended to be instantiated.</remarks>
public static class ErrorToProblemDetailsConverter
{
    /// <summary>
    /// Creates a <see cref="ProblemDetails"/> instance representing the specified error and exception for the given
    /// HTTP context and hosting environment.
    /// </summary>
    /// <param name="error">The error to be represented in the problem details. Cannot be null.</param>
    /// <param name="exception">The exception associated with the error. Cannot be null.</param>
    /// <param name="httpContext">The current HTTP context for the request. Cannot be null.</param>
    /// <param name="environment">The hosting environment in which the application is running. Cannot be null.</param>
    /// <returns>A <see cref="ProblemDetails"/> object describing the error and exception in the context of the current HTTP
    /// request.</returns>
    public static ProblemDetails GetProblemDetails(Error error, Exception exception, HttpContext httpContext, IHostEnvironment environment)
    {
        IEnumerable<Error> errors = new List<Error> { error };
        return InternalGetProblemDetails(errors.ToList(), exception, httpContext, environment);
    }

    /// <summary>
    /// Creates a <see cref="ProblemDetails"/> instance representing the specified errors and exception in the context
    /// of the current HTTP request and hosting environment.
    /// </summary>
    /// <param name="errors">A collection of <see cref="Error"/> objects that describe the errors to include in the problem details. Cannot
    /// be null.</param>
    /// <param name="exception">The exception that triggered the error response. Cannot be null.</param>
    /// <param name="httpContext">The current HTTP context associated with the request. Cannot be null.</param>
    /// <param name="environment">The hosting environment in which the application is running. Cannot be null.</param>
    /// <returns>A <see cref="ProblemDetails"/> object containing information about the errors and exception, formatted for the
    /// current HTTP context and environment.</returns>
    public static ProblemDetails GetProblemDetails(IEnumerable<Error> errors, Exception exception, HttpContext httpContext, IHostEnvironment environment)
    {
        return InternalGetProblemDetails(errors.ToList(), exception, httpContext, environment);
    }

    /// <summary>
    /// Creates a ProblemDetails instance representing the specified errors and exception for the given HTTP context and
    /// environment.
    /// </summary>
    /// <remarks>In development environments, the returned ProblemDetails includes the exception message and
    /// stack trace in the Detail property. In other environments, a generic error message is provided
    /// instead.</remarks>
    /// <param name="errors">A list of Error objects describing the errors to include in the problem details. Must contain at least one
    /// error.</param>
    /// <param name="exception">The exception that triggered the error response. Used to provide detailed information in development
    /// environments.</param>
    /// <param name="httpContext">The current HTTP context associated with the request. Used to set the instance path in the problem details.</param>
    /// <param name="environment">The hosting environment, used to determine whether to include detailed exception information.</param>
    /// <returns>A ProblemDetails object containing information about the errors, the HTTP request path, and exception details if
    /// in development mode.</returns>
    private static ProblemDetails InternalGetProblemDetails(List<Error> errors, Exception exception, HttpContext httpContext, IHostEnvironment environment)
    {
        ValidateErrorToProblemDetailsConverterParameters(errors, exception, httpContext, environment);

        Error firstError = errors.First();
        int statusCode = (int)firstError.Code;

        List<ValidationFailure> validationFailures = new(errors.Count);
        foreach (var error in errors)
        {
            validationFailures.Add(new ValidationFailure(error.Code.ToString(), error.Description));
        }

        ProblemDetails problemDetails = new(validationFailures, statusCode)
        {
            Instance = httpContext.Request.Path.ToString(),
            Detail = environment.IsDevelopment()
                ? $"{exception.Message}\n\nStack trace:\n{exception.StackTrace ?? "No stack trace available"}"
                : "One or more errors occurred. See the errors property for details."
        };

        return problemDetails;
    }

    /// <summary>
    /// Validates the parameters required for converting errors to problem details.
    /// </summary>
    /// <param name="errors">The collection of errors to be converted. Must contain at least one error and cannot be null.</param>
    /// <param name="exception">The exception associated with the error conversion. Cannot be null.</param>
    /// <param name="httpContext">The current HTTP context for the request. Cannot be null.</param>
    /// <param name="environment">The hosting environment in which the application is running. Cannot be null.</param>
    /// <exception cref="ArgumentException">Thrown if the errors collection is empty.</exception>
    private static void ValidateErrorToProblemDetailsConverterParameters(List<Error> errors, Exception exception, HttpContext httpContext, IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(errors);
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(environment);


        if (errors.Count == 0)
        {
            throw new ArgumentException("The errors collection must contain at least one error.", nameof(errors));
        }
    }
}
