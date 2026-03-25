# Code Examples

This document provides practical code examples demonstrating how to use the Clean Architecture patterns in this project.

## Table of Contents

1. [Creating a Command](#creating-a-command)
2. [Creating a Query](#creating-a-query)
3. [Using the Mediator](#using-the-mediator)
4. [Creating an Endpoint](#creating-an-endpoint)
5. [Working with Value Objects](#working-with-value-objects)
6. [Error Handling](#error-handling)
7. [Using the Result Pattern](#using-the-result-pattern)
8. [Domain Service Example](#domain-service-example)
9. [Mediator Usage Examples](#mediator-usage-examples)
10. [Pipeline Behaviors](#pipeline-behaviors)
11. [Creating a Validator](#creating-a-validator)
12. [Domain Events](#domain-events)

## Creating a Command

Commands represent write operations that change system state.

### Step 1: Define the Command

```csharp
// Application/Users/Commands/CreateUser/CreateUserCommand.cs
using Application.Abstractions.Messaging;
using Application.Users;
using Domain.Common;

namespace Application.Users.Commands.CreateUser;

public sealed class CreateUserCommand : ICommand<UserDTO>
{
    public UserRequest UserRequest { get; }

    public CreateUserCommand(UserRequest userRequest)
    {
        UserRequest = userRequest;
    }
}
```

### Step 2: Create the Command Handler

```csharp
// Application/Users/Commands/CreateUser/CreateUserCommandHandler.cs
using Application.Abstractions.Messaging;
using Application.Users.Events;
using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;

namespace Application.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler(
    IUnitOfWork unitOfWork,
    IUserCommandRepository userCommandRepository,
    IUserService userService
    ) : ICommandHandler<CreateUserCommand, UserDTO>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserCommandRepository _userCommandRepository = userCommandRepository;
    private readonly IUserService _userService = userService;

    public async Task<Result<UserDTO>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        Result<UserEntity> user = await _userService.CreateUserEntityAsync(
            request.UserRequest.Email,
            request.UserRequest.FirstName,
            request.UserRequest.LastName);

        if (user.IsFailure)
        {
            return Result<UserDTO>.Failure(user.Errors);
        }

        await _userCommandRepository.AddAsync(user);
        user.Value.RaiseDomainEvent(new UserRegisteredDomainEvent()
        {
            UserId = user.Value.Id
        });

        Result result = await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (result.IsFailure)
        {
            return Result<UserDTO>.Failure(result.Errors);
        }

        return Result<UserDTO>.Success(UserMapper.Map(user));
    }
}
```

## Creating a Query

Queries represent read operations that retrieve data without changing state.

### Step 1: Define the Query

```csharp
// Application/Users/Queries/ReadUser/ReadUserQuery.cs
using Application.Abstractions.Messaging;
using Application.Users;
using Domain.Common;

namespace Application.Users.Queries.ReadUser;

public sealed class ReadUserQuery : IQuery<UserDTO>
{
    public Guid Id { get; }

    public ReadUserQuery(Guid id)
    {
        Id = id;
    }
}
```

### Step 2: Create the Query Handler

```csharp
// Application/Users/Queries/ReadUser/ReadUserQueryHandler.cs
using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;

namespace Application.Users.Queries.ReadUser;

internal sealed class ReadUserQueryHandler : IQueryHandler<ReadUserQuery, UserDTO>
{
    private readonly IUserQueryRepository _userQueryRepository;

    public ReadUserQueryHandler(IUserQueryRepository userRepository)
    {
        _userQueryRepository = userRepository;
    }

    public async Task<Result<UserDTO>> Handle(ReadUserQuery request, CancellationToken cancellationToken)
    {
        Result<UserEntity> user = await _userQueryRepository.GetByIdAsync(request.Id);

        if (user.IsFailure)
        {
            return Result<UserDTO>.Failure(user.Errors);
        }

        return Result<UserDTO>.Success(UserMapper.Map(user));
    }
}
```

## Using the Mediator

The Mediator pattern is used to decouple endpoints from command/query handlers. Instead of directly injecting handlers, endpoints use the `IMediator` interface to send requests.

### Mediator Interface

The mediator provides two `Send` methods:

1. **`Send<TRequest>`** - For commands that return `Result` (no value)
2. **`Send<TRequest, TResponse>`** - For commands/queries that return `Result<TResponse>`

### Registration

The mediator is registered in `Application/DependencyInjection.cs`:

```csharp
services.AddScoped<IMediator, Application.Mediator.Mediator>();
```

### Benefits

- **Decoupling**: Endpoints don't need to know about specific handlers
- **Pipeline Behaviors**: All requests automatically go through registered pipeline behaviors (e.g., logging)
- **Flexibility**: Easy to add cross-cutting concerns without modifying endpoints or handlers
- **Testability**: Easy to mock the mediator in tests

## Creating an Endpoint

Endpoints handle HTTP requests and use the mediator to send commands/queries.

### Step 1: Create the Endpoint

```csharp
// WebApi/Endpoints/Users/CreateUserEndpoint.cs
using Application.Abstractions.Messaging;
using Application.Users;
using Application.Users.Commands.CreateUser;
using Domain.Common;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using WebApi.Abstractions;

namespace WebApi.Endpoints.Users;

public class CreateUserEndpoint : BaseEndpoint<UserRequest, Results<Ok<UserDTO>, NotFound, ProblemDetails>, UserDTO>
{
    private readonly IMediator _mediator;

    public CreateUserEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/users");
        AllowAnonymous(); // Or use RequireAuthorization() for protected endpoints
    }

    public override async Task<Results<Ok<UserDTO>, NotFound, ProblemDetails>> ExecuteAsync(
        UserRequest request, 
        CancellationToken cancellationToken)
    {
        CreateUserCommand createUserCommand = new(request);
        Result<UserDTO> result = await _mediator.Send<CreateUserCommand, UserDTO>(createUserCommand, cancellationToken);

        if (result.IsFailure)
        {
            return HandleErrors(result);
        }

        return TypedResults.Ok(result.Value);
    }
}
```

### Step 2: Define the Request Model

```csharp
// Application/Users/UserRequest.cs
namespace Application.Users;

public sealed record UserRequest
{
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
}
```

## Working with Value Objects

Value objects encapsulate domain concepts and ensure validation.

### Creating a Value Object

```csharp
// Domain/Entities/User/ValueObjects/Email.cs
using Domain.Common;

namespace Domain.Entities.User.ValueObjects;

public sealed record Email
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result<Email>.Failure(new List<Error> 
            { 
                new Error(ErrorCode.ValidationError, "Email cannot be empty") 
            });
        }

        if (!IsValidEmail(email))
        {
            return Result<Email>.Failure(new List<Error> 
            { 
                new Error(ErrorCode.ValidationError, "Invalid email format") 
            });
        }

        return Result<Email>.Success(new Email(email));
    }

    private static bool IsValidEmail(string email)
    {
        // Use regex or validation library
        return System.Text.RegularExpressions.Regex.IsMatch(
            email, 
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
}
```

### Using Value Objects in Entities

```csharp
// Domain/Entities/User/UserEntity.cs
public sealed class UserEntity : BaseEntity
{
    public Email Email { get; private set; } = default!;
    public FirstName FirstName { get; private set; } = default!;
    public LastName LastName { get; private set; } = default!;

    private UserEntity(Email email, FirstName firstName, LastName lastName) : base()
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
    }

    public static UserEntity Create(Email email, FirstName firstName, LastName lastName)
    {
        return new UserEntity(email, firstName, lastName);
    }
}
```

## Error Handling

### Creating Domain Errors

```csharp
// Domain/Common/GenericErrors.cs (example)
using Domain.Common;

namespace Domain.Common;

public static class GenericErrors
{
    public static Error NotFound(Guid id) => 
        new Error(ErrorCode.NotFound, $"Entity with ID {id} was not found.");

    public static Error ValidationFailed(string message) => 
        new Error(ErrorCode.ValidationError, message);

    public static Error Conflict(string message) => 
        new Error(ErrorCode.Conflict, message);
}
```

### Handling Errors in Handlers

```csharp
public async Task<Result<UserDTO>> Handle(ReadUserQuery request, CancellationToken cancellationToken)
{
    Result<UserEntity> user = await _userQueryRepository.GetByIdAsync(request.Id);

    if (user.IsFailure)
    {
        // Return errors from repository
        return Result<UserDTO>.Failure(user.Errors);
    }

    // Additional validation
    if (user.Value.IsDeleted)
    {
        return Result<UserDTO>.Failure(new List<Error> 
        { 
            GenericErrors.NotFound(request.Id) 
        });
    }

    return Result<UserDTO>.Success(UserMapper.Map(user));
}
```

### Error Handling in Endpoints

The `BaseEndpoint` class provides error handling:

```csharp
// BaseEndpoint automatically converts Result errors to ProblemDetails
if (result.IsFailure)
{
    return HandleErrors(result); // Converts to HTTP ProblemDetails
}
```

### Global Exception Handling

The application uses a `GlobalExceptionHandler` that catches all unhandled exceptions. This complements the Result pattern by handling unexpected exceptions that escape the normal flow.

**How it works:**
- All unhandled exceptions are automatically caught by the `GlobalExceptionHandler`
- Exceptions are converted to standardized `ProblemDetails` responses
- Different exception types are mapped to appropriate HTTP status codes
- In development mode, detailed error information (including stack traces) is returned
- In production mode, generic error messages are returned for security

**Exception Type Mappings:**
```csharp
// The handler automatically maps exceptions to HTTP status codes:
ArgumentException / ArgumentNullException → 400 (ValidationError)
UnauthorizedAccessException → 401 (Unauthorized)
KeyNotFoundException → 404 (NotFound)
InvalidOperationException / NotSupportedException → 501 (NotImplemented)
Other exceptions → 500 (InternalServerError)
```

**Example: Unhandled Exception**

If an unexpected exception occurs in your code:

```csharp
public async Task<Result<UserDTO>> Handle(ReadUserQuery request, CancellationToken cancellationToken)
{
    // If this throws an unexpected exception (e.g., NullReferenceException),
    // it will be caught by GlobalExceptionHandler
    var user = await _userQueryRepository.GetByIdAsync(request.Id);
    
    // ... rest of the code
}
```

The `GlobalExceptionHandler` will:
1. Log the exception with full context (request path, trace ID)
2. Convert it to a ProblemDetails response
3. Return appropriate HTTP status code
4. Include detailed information in development, generic message in production

**Best Practice:**
- Use Result pattern for expected business errors (validation, not found, etc.)
- Let GlobalExceptionHandler catch unexpected exceptions (system errors, null references, etc.)
- Never manually catch and swallow exceptions unless you have a specific reason

## Using the Result Pattern

### Chaining Operations

```csharp
public async Task<Result<UserDTO>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
{
    // Step 1: Validate email
    Result<Email> emailResult = Email.Create(request.UserRequest.Email);
    if (emailResult.IsFailure)
    {
        return Result<UserDTO>.Failure(emailResult.Errors);
    }

    // Step 2: Validate first name
    Result<FirstName> firstNameResult = FirstName.Create(request.UserRequest.FirstName);
    if (firstNameResult.IsFailure)
    {
        return Result<UserDTO>.Failure(firstNameResult.Errors);
    }

    // Step 3: Validate last name
    Result<LastName> lastNameResult = LastName.Create(request.UserRequest.LastName);
    if (lastNameResult.IsFailure)
    {
        return Result<UserDTO>.Failure(lastNameResult.Errors);
    }

    // Step 4: Create entity
    UserEntity user = UserEntity.Create(emailResult.Value, firstNameResult.Value, lastNameResult.Value);

    // Step 5: Save
    await _userCommandRepository.AddAsync(user);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return Result<UserDTO>.Success(UserMapper.Map(user));
}
```

### Combining Multiple Errors

```csharp
public static Result<UserEntity> CreateUserEntity(string email, string firstName, string lastName)
{
    var errors = new List<Error>();

    Result<Email> emailResult = Email.Create(email);
    if (emailResult.IsFailure)
    {
        errors.AddRange(emailResult.Errors);
    }

    Result<FirstName> firstNameResult = FirstName.Create(firstName);
    if (firstNameResult.IsFailure)
    {
        errors.AddRange(firstNameResult.Errors);
    }

    Result<LastName> lastNameResult = LastName.Create(lastName);
    if (lastNameResult.IsFailure)
    {
        errors.AddRange(lastNameResult.Errors);
    }

    if (errors.Any())
    {
        return Result<UserEntity>.Failure(errors);
    }

    return Result<UserEntity>.Success(
        UserEntity.Create(emailResult.Value, firstNameResult.Value, lastNameResult.Value)
    );
}
```

## Domain Service Example

Domain services contain business logic that doesn't belong to a single entity.

### Step 1: Define the Domain Service Interface

```csharp
// Domain/Entities/User/Interfaces/IUserService.cs (excerpt)
using Domain.Common;
using Domain.Entities.User;

namespace Domain.Entities.User.Interfaces;

public interface IUserService
{
    Task<Result<UserEntity>> CreateUserEntityAsync(string email, string firstName, string lastName);
    // … other members — see source file
}
```

### Step 2: Implement the Domain Service

The real `UserService` validates value objects, checks email availability via `IUserQueryRepository`, and returns `Task<Result<UserEntity>>`. A simplified shape:

```csharp
// Application/Users/Services/UserServices.cs (conceptual excerpt)
using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;
using Domain.Entities.User.ValueObjects;

namespace Application.Users.Services;

public sealed class UserService : IUserService
{
    public Task<Result<UserEntity>> CreateUserEntityAsync(string email, string firstName, string lastName)
    {
        Result<Email> emailResult = Email.Create(email);
        if (emailResult.IsFailure)
        {
            return Task.FromResult(Result<UserEntity>.Failure(emailResult.Errors));
        }

        Result<FirstName> firstNameResult = FirstName.Create(firstName);
        if (firstNameResult.IsFailure)
        {
            return Task.FromResult(Result<UserEntity>.Failure(firstNameResult.Errors));
        }

        Result<LastName> lastNameResult = LastName.Create(lastName);
        if (lastNameResult.IsFailure)
        {
            return Task.FromResult(Result<UserEntity>.Failure(lastNameResult.Errors));
        }

        // Real implementation also checks uniqueness and uses IUserQueryRepository — see UserServices.cs
        return Task.FromResult(Result<UserEntity>.Success(
            UserEntity.Create(emailResult.Value, firstNameResult.Value, lastNameResult.Value)
        ));
    }
}
```

### Step 3: Register the Service

```csharp
// Application/DependencyInjection.cs
services.AddScoped<IUserService, UserService>();
```

## Mapping Entities to DTOs

DTOs are used to transfer data between layers without exposing domain entities.

### Create a Mapper

```csharp
// Application/Users/UserMapper.cs
using Domain.Entities.User;

namespace Application.Users;

public static class UserMapper
{
    public static UserDTO Map(UserEntity user)
    {
        return new UserDTO
        {
            Id = user.Id,
            Email = user.Email.Value,
            FirstName = user.FirstName.Value,
            LastName = user.LastName.Value,
            CreatedAt = user.CreatedAt,
            LastUpdatedAt = user.LastUpdatedAt
        };
    }

    public static UserDTO Map(Result<UserEntity> userResult)
    {
        return Map(userResult.Value);
    }
}
```

### Define the DTO

```csharp
// Application/Users/UserDTO.cs
namespace Application.Users;

public sealed class UserDTO
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime LastUpdatedAt { get; init; }
}
```

## Complete Example: Update User Command

Here's a complete example combining all patterns:

### Command

```csharp
public record UpdateUserCommand(UserRequest UserRequest) : ICommand<UserDTO>;
```

### Handler

```csharp
public sealed class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, UserDTO>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserCommandRepository _userCommandRepository;
    private readonly IUserQueryRepository _userQueryRepository;
    private readonly IUserService _userService;

    public UpdateUserCommandHandler(
        IUnitOfWork unitOfWork,
        IUserCommandRepository userCommandRepository,
        IUserQueryRepository userQueryRepository,
        IUserService userService)
    {
        _unitOfWork = unitOfWork;
        _userCommandRepository = userCommandRepository;
        _userQueryRepository = userQueryRepository;
        _userService = userService;
    }

    public async Task<Result<UserDTO>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        // Get existing user
        Result<UserEntity> existingUser = await _userQueryRepository.GetByIdAsync(request.Id);
        if (existingUser.IsFailure)
        {
            return Result<UserDTO>.Failure(existingUser.Errors);
        }

        // Validate new values
        Result<Email> emailResult = Email.Create(request.UserRequest.Email);
        Result<FirstName> firstNameResult = FirstName.Create(request.UserRequest.FirstName);
        Result<LastName> lastNameResult = LastName.Create(request.UserRequest.LastName);

        var errors = new List<Error>();
        if (emailResult.IsFailure) errors.AddRange(emailResult.Errors);
        if (firstNameResult.IsFailure) errors.AddRange(firstNameResult.Errors);
        if (lastNameResult.IsFailure) errors.AddRange(lastNameResult.Errors);

        if (errors.Any())
        {
            return Result<UserDTO>.Failure(errors);
        }

        // Update entity
        existingUser.Value.Update(emailResult.Value, firstNameResult.Value, lastNameResult.Value);
        _userCommandRepository.Update(existingUser.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UserDTO>.Success(UserMapper.Map(existingUser));
    }
}
```

### Endpoint

```csharp
public class UpdateUserEndpoint : BaseEndpoint<UserRequest, Results<Ok<UserDTO>, NotFound, ProblemDetails>, UserDTO>
{
    private readonly IMediator _mediator;

    public UpdateUserEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/api/users/{id}");
        AllowAnonymous();
    }

    public override async Task<Results<Ok<UserDTO>, NotFound, ProblemDetails>> ExecuteAsync(
        UserRequest request, 
        CancellationToken cancellationToken)
    {
        Guid id = Route<Guid>("id");
        UpdateUserCommand command = new(id, request);
        Result<UserDTO> result = await _mediator.Send<UpdateUserCommand, UserDTO>(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleErrors(result);
        }

        return TypedResults.Ok(result.Value);
    }
}
```

## Mediator Usage Examples

### Example 1: Command with Return Value

```csharp
// Endpoint using mediator to send a command that returns a value
public class CreateUserEndpoint : BaseEndpoint<UserRequest, Results<Ok<UserDTO>, ProblemDetails>, UserDTO>
{
    private readonly IMediator _mediator;

    public CreateUserEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<Results<Ok<UserDTO>, ProblemDetails>> ExecuteAsync(
        UserRequest request, 
        CancellationToken cancellationToken)
    {
        CreateUserCommand command = new(request);
        Result<UserDTO> result = await _mediator.Send<CreateUserCommand, UserDTO>(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleErrors(result);
        }

        return TypedResults.Ok(result.Value);
    }
}
```

### Example 2: Command without Return Value

```csharp
// Endpoint using mediator to send a command that doesn't return a value
public class DeleteUserEndpoint : BaseEndpoint<EmptyRequest, Results<NoContent, ProblemDetails>, UserDTO>
{
    private readonly IMediator _mediator;

    public DeleteUserEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<Results<NoContent, ProblemDetails>> ExecuteAsync(
        EmptyRequest request, 
        CancellationToken cancellationToken)
    {
        string? id = Route<string>("id");
        if (id is null || !Guid.TryParse(id, out Guid userId))
        {
            return GetNotFoundResponse();
        }

        DeleteUserCommand command = new(userId);
        Result result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleErrors(result);
        }

        return TypedResults.NoContent();
    }
}
```

### Example 3: Query

```csharp
// Endpoint using mediator to send a query
public class GetUserEndpoint : BaseEndpoint<EmptyRequest, Results<Ok<UserDTO>, ProblemDetails>, UserDTO>
{
    private readonly IMediator _mediator;

    public GetUserEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<Results<Ok<UserDTO>, ProblemDetails>> ExecuteAsync(
        EmptyRequest request, 
        CancellationToken cancellationToken)
    {
        string? id = Route<string>("id");
        if (id is null || !Guid.TryParse(id, out Guid userId))
        {
            return GetNotFoundResponse();
        }

        ReadUserQuery query = new(userId);
        Result<UserDTO> result = await _mediator.Send<ReadUserQuery, UserDTO>(query, cancellationToken);

        if (result.IsFailure)
        {
            return HandleErrors(result);
        }

        return TypedResults.Ok(result.Value);
    }
}
```

### Example 4: Query Returning a List

```csharp
// Endpoint using mediator to send a query that returns a list
public class GetUsersEndpoint : BaseEndpoint<EmptyRequest, Results<Ok<IList<UserDTO>>, ProblemDetails>, IList<UserDTO>>
{
    private readonly IMediator _mediator;

    public GetUsersEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<Results<Ok<IList<UserDTO>>, ProblemDetails>> ExecuteAsync(
        EmptyRequest request, 
        CancellationToken cancellationToken)
    {
        ReadUserListQuery query = new();
        Result<IList<UserDTO>> result = await _mediator.Send<ReadUserListQuery, IList<UserDTO>>(query, cancellationToken);

        if (result.IsFailure)
        {
            return HandleErrors(result);
        }

        return TypedResults.Ok(result.Value);
    }
}
```

## Pipeline Behaviors

The mediator automatically applies registered pipeline behaviors to all requests. This allows cross-cutting concerns like logging, validation, and caching to be applied automatically.

### How Pipeline Behaviors Work

When you send a request through the mediator:

1. The mediator finds the appropriate handler
2. It builds a pipeline of registered behaviors
3. Each behavior wraps the next in the chain
4. The request flows through all behaviors before reaching the handler
5. The response flows back through all behaviors

### Example: Validation Behavior

The `ValidationPipelineBehavior` automatically validates all requests using registered validators before they reach handlers.

**Key Points:**
- Validators are automatically discovered and executed
- Multiple validators can be registered for the same request type
- If any validator fails, the handler is never called
- All validation errors are collected and returned together

### Example: Logging Behavior

The `LoggingPipelineBehavior` automatically logs all requests and responses:

```csharp
// This behavior is automatically applied to all requests
public class LoggingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger _logger;

    public async Task<TResponse> HandleAsync(
        TRequest request, 
        CancellationToken cancellationToken, 
        Func<Task<TResponse>> next)
    {
        _logger.LogInformation("Handling {RequestType}", typeof(TRequest).Name);
        
        var stopwatch = Stopwatch.StartNew();
        var result = await next();
        stopwatch.Stop();
        
        _logger.LogInformation("Completed {RequestType} in {ElapsedMs}ms", 
            typeof(TRequest).Name, stopwatch.ElapsedMilliseconds);
        
        return result;
    }
}
```

### Registering Pipeline Behaviors

Pipeline behaviors are registered in `Application/DependencyInjection.cs`:

```csharp
// Behaviors - Order matters! Validation should run before logging
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehavior<,>));

// Validators are automatically discovered and registered
services.Scan(scan => scan.FromAssembliesOf(typeof(DependencyInjection))
    .AddClasses(classes => classes.AssignableTo(typeof(IValidator<>)), publicOnly: false)
        .AsImplementedInterfaces()
        .WithScopedLifetime()
);
```

**Pipeline Execution Order:**
1. **ValidationPipelineBehavior** - Runs first, validates input
2. **LoggingPipelineBehavior** - Logs the request (only if validation passes)
3. **Handler** - Processes the request (only if validation passes)

The order of registration matters - behaviors are applied in reverse order of registration, so ValidationPipelineBehavior must be registered first.

## Creating a Validator

Validators provide early validation feedback for commands and queries before they reach their handlers. They validate input format, basic constraints, and can check business rules (like uniqueness). Validators are automatically executed by the `ValidationPipelineBehavior` in the request pipeline.

**Key Points:**
- Validators run **before** handlers execute
- They complement domain validations (value objects still validate at the domain layer)
- Multiple validators can be registered for the same request type
- If validation fails, the handler is never called

### Step 1: Implement the IValidator Interface

```csharp
// Application/Users/Commands/CreateUser/CreateUserCommandValidator.cs
using Application.Abstractions.Validation;
using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;
using Domain.Entities.User.ValueObjects;

namespace Application.Users.Commands.CreateUser;

/// <summary>
/// Validates CreateUserCommand input before it reaches the handler.
/// This provides early validation feedback for input format and basic constraints.
/// 
/// Note: This is complementary to domain object validations. Domain validations
/// (Email.Create, FirstName.Create, etc.) enforce business rules and invariants.
/// This validator focuses on input validation at the application boundary.
/// </summary>
public class CreateUserCommandValidator : IValidator<CreateUserCommand>
{
    private readonly IUserQueryRepository _userQueryRepository;

    public CreateUserCommandValidator(IUserQueryRepository userQueryRepository)
    {
        _userQueryRepository = userQueryRepository;
    }

    public async Task<Result> ValidateAsync(
        CreateUserCommand request, 
        CancellationToken cancellationToken = default)
    {
        List<Error> errors = new();

        // Validate email format and availability
        List<Error> emailErrors = await ValidateEmailAsync(request.UserRequest.Email);
        errors.AddRange(emailErrors);

        // Validate first name
        if (string.IsNullOrWhiteSpace(request.UserRequest.FirstName))
        {
            errors.Add(UserErrors.EmptyName("FirstName"));
        }
        else if (request.UserRequest.FirstName.Length < 2 || 
                 request.UserRequest.FirstName.Length > 50)
        {
            errors.Add(UserErrors.InvalidNameLength("FirstName"));
        }

        // Validate last name
        if (string.IsNullOrWhiteSpace(request.UserRequest.LastName))
        {
            errors.Add(UserErrors.EmptyName("LastName"));
        }
        else if (request.UserRequest.LastName.Length < 2 || 
                 request.UserRequest.LastName.Length > 50)
        {
            errors.Add(UserErrors.InvalidNameLength("LastName"));
        }

        return errors.Any() 
            ? Result.Failure(errors) 
            : Result.Success();
    }

    private async Task<List<Error>> ValidateEmailAsync(string email)
    {
        List<Error> errors = new();

        // Check if email is empty
        if (string.IsNullOrWhiteSpace(email))
        {
            errors.Add(UserErrors.EmptyEmail());
            return errors; // Early return if empty
        }

        // Check email format using domain value object
        if (!Email.IsValid(email))
        {
            errors.Add(UserErrors.InvalidEmail());
            return errors; // Early return if invalid format
        }

        // Create email value object for further validation
        Result<Email> emailResult = Email.Create(email);
        if (emailResult.IsFailure)
        {
            errors.AddRange(emailResult.Errors);
            return errors;
        }

        // Check if email is already in use (business rule validation)
        Result<UserEntity> existingUser = await _userQueryRepository
            .GetByEmailAsync(emailResult.Value);
        
        if (existingUser.IsSuccess)
        {
            errors.Add(UserErrors.EmailAlreadyInUse(email));
        }

        return errors;
    }
}
```

### Automatic Registration

Validators are automatically discovered and registered by Scrutor in `Application/DependencyInjection.cs`.

**Benefits of Automatic Registration:**
- No manual registration required for each validator
- Validators are automatically discovered from the assembly
- Multiple validators for the same request type are all executed

### What Happens When Validation Fails?

When a validator returns `Result.Failure`:

1. **ValidationPipelineBehavior** collects all errors from all validators
2. **Handler is never called** - execution stops in the pipeline
3. **Error response is returned** - `Result<TResponse>.Failure(errors)` is returned
4. **Endpoint receives failure** - endpoint's `HandleErrors(result)` method converts errors to HTTP ProblemDetails

**Example Flow:**
```
Request → ValidationPipelineBehavior → Validator fails → 
Return Result.Failure(errors) → Endpoint → HandleErrors → 
HTTP 400 Bad Request with ProblemDetails
```

### Relationship Between Validators and Domain Validations

| Aspect | Validators | Domain Validations (Value Objects) |
|--------|-----------|-----------------------------------|
| **When** | Before handler executes | During handler execution |
| **Purpose** | Input validation, format checks, basic constraints | Business rules, invariants, domain logic |
| **Performance** | Can prevent expensive operations | Always runs as part of business logic |
| **External Data** | Can check uniqueness, availability | Typically pure logic |
| **Errors** | Early feedback to API consumers | Part of business rule enforcement |

**Both are important and complement each other!**

## Domain Events

Domain events let a use case notify other code about something that already happened, without putting all side effects inside the command handler. Contracts (`IDomainEvent`, `IDomainEventHandler<>`, `IDomainEventsDispatcher`) live in **Domain**; event records and handlers live under **Application** (for example `Application/Users/Events/`). Entities inherit `RaiseDomainEvent` from **`BaseEntity`**; **`UnitOfWork`** collects events from tracked entities, persists, then dispatches after a successful commit.

### Step 1: Define the event

```csharp
// Application/Users/Events/UserRegisteredDomainEvent.cs
using Domain.Interfaces;

namespace Application.Users.Events;

internal sealed record UserRegisteredDomainEvent : IDomainEvent
{
    public Guid UserId { get; init; }
}
```

### Step 2: Implement a handler

```csharp
// Application/Users/Events/SendWelcomeEmailHandler.cs
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Users.Events;

internal class SendWelcomeEmailHandler(ILogger<SendWelcomeEmailHandler> logger)
    : IDomainEventHandler<UserRegisteredDomainEvent>
{
    private readonly ILogger<SendWelcomeEmailHandler> _logger = logger;

    public Task Handle(UserRegisteredDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending welcome email to user with ID: {UserId}", domainEvent.UserId);
        return Task.CompletedTask;
    }
}
```

### Step 3: Register in DI

Register `IDomainEventsDispatcher` once. Register `IDomainEventHandler<>` implementations with **Scrutor** (same assembly scan as command and query handlers):

```csharp
// Application/DependencyInjection.cs (excerpt)
services.AddScoped<IDomainEventsDispatcher, DomainEventsDispatcher>();

services.Scan(scan => scan.FromAssembliesOf(typeof(DependencyInjection))
    // ... command handlers, query handlers ...
    .AddClasses(classes => classes.AssignableTo(typeof(IDomainEventHandler<>)), publicOnly: false)
        .AsImplementedInterfaces()
        .WithScopedLifetime()
);
```

### Step 4: Raise on the entity; persistence dispatches

Call `RaiseDomainEvent` on the entity **before** `SaveChangesAsync` (see [Creating a Command](#creating-a-command)). Do not call `DispatchAsync` from the command handler. Map `DomainEvents` as ignored in EF (`builder.Ignore(x => x.DomainEvents)` in the entity configuration). See [Design Patterns: Domain Events](../architecture/design_patterns.md#11-domain-events) for ordering and post-commit considerations.

## Best Practices Demonstrated

1. **Separation of Concerns**: Each layer has clear responsibilities
2. **Error Handling**: Use Result pattern, not exceptions for business logic
3. **Validation**: Validate at multiple levels - validators for input validation, domain value objects for business rules
4. **Immutability**: Value objects and entities use private setters
5. **Factory Methods**: Entities created via static factory methods
6. **DTOs**: Domain entities never exposed to API layer
7. **Dependency Injection**: All dependencies injected via constructor
8. **Mediator Pattern**: Use mediator to decouple endpoints from handlers
9. **Pipeline Behaviors**: Use behaviors for cross-cutting concerns (validation, logging)
10. **Early Validation**: Use validators to provide fast feedback and prevent invalid data from reaching handlers
11. **Domain events**: Raise on `BaseEntity` before `SaveChanges`; let `UnitOfWork` dispatch after successful persistence; keep handlers out of the mediator pipeline unless you intentionally unify them

