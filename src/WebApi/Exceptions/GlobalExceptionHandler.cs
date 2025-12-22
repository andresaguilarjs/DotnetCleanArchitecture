using Domain.Common;
using FastEndpoints;
using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;
using WebApi.Common;

namespace WebApi.Exceptions;

/// <summary>
/// Global exception handler for the application using IExceptionHandler interface (.NET 10).
/// This handler uses FastEndpoints.ProblemDetails for consistent error responses across the application.
/// <param name="_logger">The logger to use for logging the exception.</param>
/// <param name="_environment">The environment to use for determining if the application is in development mode.</param>
internal sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> _logger,
    IHostEnvironment _environment
) : IExceptionHandler
{
    private JsonSerializerOptions? ResponseConfiguration { get; set; }

    /// <summary>
    /// Attempts to handle the exception and write a response.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="exception">The exception to handle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the exception was handled; otherwise, false.</returns>
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        LogError(httpContext, exception);
        if (IsResponseStarted(httpContext))
        {
            return false;
        }

        ProblemDetails problemDetails = GetProblemDetailsFromException(exception, httpContext);

        httpContext.Response.StatusCode = problemDetails.Status;
        httpContext.Response.ContentType = "application/problem+json";
        await WriteErrorResponse(httpContext, problemDetails, cancellationToken);

        return true;
    }

    /// <summary>
    /// Creates a ProblemDetails instance that represents the specified exception in the context of the given HTTP
    /// request.
    /// </summary>
    /// <param name="exception">The exception to convert to a ProblemDetails response. Cannot be null.</param>
    /// <param name="httpContext">The current HTTP context associated with the request. Cannot be null.</param>
    /// <returns>A ProblemDetails object containing details about the exception suitable for use in an HTTP response.</returns>
    private ProblemDetails GetProblemDetailsFromException(Exception exception, HttpContext httpContext)
    {
        Error error = GetGenericError(exception);
        return ErrorToProblemDetailsConverter.GetProblemDetails(error, exception, httpContext, _environment);
    }

    private Error GetGenericError(Exception exception)
    {
        return exception switch
        {
            ArgumentException or ArgumentNullException => GenericErrors.ArgumentError(),
            UnauthorizedAccessException => GenericErrors.Unauthorized(),
            KeyNotFoundException => GenericErrors.NotFound(),
            InvalidOperationException => GenericErrors.NotImplemented(),
            NotSupportedException => GenericErrors.NotImplemented(),
            _ => GenericErrors.SomethingWhenWrong()
        };
    }

    /// <summary>
    /// Retrieves the JSON serializer options used for response serialization.
    /// </summary>
    /// <remarks>If no response configuration has been set, a default configuration with camel case property
    /// naming is created and returned. Subsequent calls return the same instance.</remarks>
    /// <returns>A <see cref="JsonSerializerOptions"/> instance that specifies the configuration for serializing responses.</returns>
    private JsonSerializerOptions GetResponseConfiguration()
    {
        if (ResponseConfiguration == null)
        {
            ResponseConfiguration = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }
        return ResponseConfiguration;
    }
    /// <summary>
    /// Determines whether the HTTP response has already started for the specified context.
    /// If the response has started, a warning is logged.
    /// </summary>
    /// <param name="httpContext">The HTTP context to check for a started response. Cannot be null.</param>
    /// <returns>true if the response has already started; otherwise, false.</returns>
    private bool IsResponseStarted(HttpContext httpContext)
    {
        if (httpContext.Response.HasStarted)
        {
            _logger.LogWarning("Response has already started, cannot write error response: {TraceIdentifier}", httpContext.TraceIdentifier);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Logs an unhandled exception that occurred during the processing of an HTTP request.
    /// </summary>
    /// <param name="httpContext">The HTTP context associated with the current request. Cannot be null.</param>
    /// <param name="exception">The exception to log. Cannot be null.</param>
    private void LogError(HttpContext httpContext, Exception exception)
    {
        _logger.LogError(
            exception,
            "An unhandled exception occurred. Request: {Method} {Path}, {TraceIdentifier}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            httpContext.TraceIdentifier
        );
    }

    /// <summary>
    /// Asynchronously writes a JSON error response to the HTTP response body using the specified problem details.
    /// </summary>
    /// <param name="httpContext">The HTTP context for the current request. The response body is written to this context's response stream.</param>
    /// <param name="problemDetails">The problem details object containing information about the error to be serialized in the response.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    private async ValueTask WriteErrorResponse(HttpContext httpContext, ProblemDetails problemDetails, CancellationToken cancellationToken)
    {
        await JsonSerializer.SerializeAsync(
            httpContext.Response.Body,
            problemDetails,
            GetResponseConfiguration(),
            cancellationToken
        );
    }
}