using Application.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace Application.Extensions;

/// <summary>
/// Adds custom middleware to the application pipeline.
/// </summary>
/// <param name="app">The <see cref="IApplicationBuilder"/> instance.</param>
public static class ApplicationBuilderExtensions
{
    public static void UseCustomMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHanddlerMiddleware>();
    }
}
