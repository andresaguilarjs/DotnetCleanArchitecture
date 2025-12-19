using Application;
using FastEndpoints;
using Infrastructure;
using Scalar.AspNetCore;
using WebApi;
using WebApi.Exceptions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();
builder.Services.AddFastEndpoints();
builder.Services.AddProblemDetails();

// Add services from other projects
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPresentation();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "WebApi API Reference";
        options.WithTheme(ScalarTheme.DeepSpace);
        options.WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.HttpClient);

    });
}

app.UseExceptionHandler();
app.MapHealthChecks("/health");
app.UseFastEndpoints();

app.Run();
