using Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Exceptions;

/// <summary>
/// Global exception handler for the application using IExceptionHandler interface (.NET 10).
/// This handler is used to handle all unhandled exceptions in the application.
/// </summary>
/// <param name="_logger">The logger to use for logging the exception.</param>
/// <param name="_problemDetailsService">The problem details service to use for writing the problem details to the response.</param>
/// <param name="_environment">The environment to use for determining if the application is in development mode.</param>
internal sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> _logger,
    IProblemDetailsService _problemDetailsService,
    IHostEnvironment _environment
    ) : IExceptionHandler
{

    /// <summary>
    /// Attempts to handle the exception and write a response.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="exception">The exception to handle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the exception was handled; otherwise, false.</returns>
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(
               exception,
               "An unhandled exception occurred. Request: {Method} {Path}",
               httpContext.Request.Path,
               httpContext.TraceIdentifier
        );

        if (httpContext.Response.HasStarted)
        {
            _logger.LogWarning("Response has already started, cannot write error response.");
            return false;
        }

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = GetProblemDetails(exception, httpContext)
        });
    }

    /// <summary>
    /// Gets the problem details for the exception.
    /// </summary>
    /// <param name="exception">The exception to get the problem details for.</param>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>The problem details.</returns>
    private ProblemDetails GetProblemDetails(Exception exception, HttpContext httpContext)
    {
        Error error = GetGenericError(exception);

        return new ProblemDetails
        {
            Status = (int)error.Code,
            Title = GetTitle((int)error.Code),
            Type = GetTypeUri((int)error.Code),
            Detail = _environment.IsDevelopment()
                ? $"{exception.Message}\n\nStack trace:\n{exception.StackTrace ?? "No stack trace available"}"
                : GenericErrors.SomethingWhenWrong().Description,
            Instance = httpContext.Request.Path
        };
    }

    /// <summary>
    /// Gets the generic error for the exception.
    /// </summary>
    /// <param name="exception">The exception to get the generic error for.</param>
    /// <returns>The generic error.</returns>
    private static Error GetGenericError(Exception exception)
    {
        return exception switch
        {
            ArgumentException or ArgumentNullException => GenericErrors.ArgumentError(),
            UnauthorizedAccessException => GenericErrors.Unauthorized(),
            KeyNotFoundException => GenericErrors.NotFound(),
            NotImplementedException => GenericErrors.NotImplemented(),
            _ => GenericErrors.SomethingWhenWrong()
        };
    }

    /// <summary>
    /// Gets the title for the status code.
    /// </summary>
    /// <param name="statusCode">The status code to get the title for.</param>
    /// <returns>The title.</returns>
    private static string GetTitle(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status500InternalServerError => "Internal Server Error",
            StatusCodes.Status501NotImplemented => "Not Implemented",
            _ => "An Error Occurred"
        };
    }

    /// <summary>
    /// Gets the type URI for the status code.
    /// </summary>
    /// <param name="statusCode">The status code to get the type URI for.</param>
    /// <returns>The type URI.</returns>
    private static string GetTypeUri(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            StatusCodes.Status401Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
            StatusCodes.Status404NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            StatusCodes.Status500InternalServerError => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            StatusCodes.Status501NotImplemented => "https://tools.ietf.org/html/rfc7231#section-6.6.2",
            _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };
    }
}
