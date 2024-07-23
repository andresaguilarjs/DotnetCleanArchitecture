using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Application.Middlewares;

/// <summary>
/// Middleware for handling exceptions in the application.
/// </summary>
public sealed class ExceptionHanddlerMiddleware : IMiddleware
{
    private readonly ILogger _logger;

    public ExceptionHanddlerMiddleware(ILogger<ExceptionHanddlerMiddleware> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware to handle exceptions.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="next">The delegate representing the next middleware in the pipeline.</param>
    /// <returns>A task that represents the asynchronous middleware operation.</returns>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try {
            await next(context);
        } catch (Exception exception) {
            _logger.LogError(exception, exception.Message);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = "Something went wrong. Try again later." }));
        }
    }
}