# Design Patterns

This document describes the design patterns implemented in this Clean Architecture project.

## Overview

The project implements several design patterns to achieve separation of concerns, maintainability, and testability:

1. **CQRS (Command Query Responsibility Segregation)**
2. **Mediator Pattern**
3. **Repository Pattern**
4. **Result Pattern**
5. **Pipeline Behavior Pattern**
6. **Unit of Work Pattern**
7. **Global Exception Handling Pattern**
8. **Value Object Pattern**
9. **Factory Pattern**

## 1. CQRS (Command Query Responsibility Segregation)

### Purpose
Separates read operations (queries) from write operations (commands) to optimize for different use cases and improve scalability.

### Implementation

#### Commands (Write Operations)
Commands represent operations that change the system state:

```csharp
// Command interface
public interface ICommand<TResult> : IBaseCommand;

// Example: CreateUserCommand
public sealed class CreateUserCommand : ICommand<UserDTO>
{
    public UserRequest UserRequest { get; }
    // ...
}

// Command handler
public sealed class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, UserDTO>
{
    public async Task<Result<UserDTO>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

#### Queries (Read Operations)
Queries represent operations that retrieve data without changing state:

```csharp
// Query interface
public interface IQuery<TResponse> : IBaseQuery;

// Example: ReadUserQuery
public sealed class ReadUserQuery : IQuery<UserDTO>
{
    public Guid Id { get; }
    // ...
}

// Query handler
public sealed class ReadUserQueryHandler : IQueryHandler<ReadUserQuery, UserDTO>
{
    public async Task<Result<UserDTO>> Handle(ReadUserQuery request, CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

### Benefits
- **Separation of concerns**: Read and write operations are clearly separated
- **Optimization**: Can optimize read and write paths independently
- **Scalability**: Can scale read and write operations separately
- **Clarity**: Makes intent explicit in code

### Location
- Commands: `Application/{Feature}/Commands/`
- Queries: `Application/{Feature}/Queries/`
- Interfaces: `Application/Abstractions/Messaging/`

## 2. Mediator Pattern

### Purpose
Decouples request senders from request handlers, allowing multiple handlers to process requests through a pipeline. The mediator acts as a central hub for routing commands and queries to their respective handlers while applying cross-cutting concerns through pipeline behaviors.

### Implementation

#### Mediator Interface

The mediator provides two `Send` methods:

```csharp
public interface IMediator
{
    // For commands that return Result (no value)
    Task<Result> Send<TRequest>(TRequest request, CancellationToken cancellationToken) 
        where TRequest : ICommand;
    
    // For commands/queries that return Result<TResponse>
    Task<Result<TResponse>> Send<TRequest, TResponse>(
        TRequest request, 
        CancellationToken cancellationToken) 
        where TRequest : IBaseRequest;
}
```

#### Handler Interfaces

```csharp
// Command handler interface (with return value)
public interface ICommandHandler<TCommand, TResult>
{
    Task<Result<TResult>> Handle(TCommand command, CancellationToken cancellationToken);
}

// Command handler interface (no return value)
public interface ICommandHandler<TCommand>
{
    Task<Result> Handle(TCommand command, CancellationToken cancellationToken);
}

// Query handler interface
public interface IQueryHandler<TQuery, TResponse>
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}
```

#### Registration

The mediator and handlers are registered in dependency injection:

```csharp
// Register mediator
services.AddScoped<IMediator, Application.Mediator.Mediator>();

// Register command handlers
services.AddScoped<ICommandHandler<CreateUserCommand, UserDTO>, CreateUserCommandHandler>();
services.AddScoped<ICommandHandler<DeleteUserCommand>, DeleteUserCommandHandler>();

// Register query handlers
services.AddScoped<IQueryHandler<ReadUserQuery, UserDTO>, ReadUserQueryHandler>();
services.AddScoped<IQueryHandler<ReadUserListQuery, IList<UserDTO>>, ReadUserListQueryHandler>();
```

### Usage in Endpoints

Endpoints inject `IMediator` and use it to send commands/queries:

#### Example 1: Command with Return Value

```csharp
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
        Result<UserDTO> result = await _mediator.Send<CreateUserCommand, UserDTO>(
            command, 
            cancellationToken);

        if (result.IsFailure)
        {
            return HandleErrors(result);
        }

        return TypedResults.Ok(result.Value);
    }
}
```

#### Example 2: Command without Return Value

```csharp
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

#### Example 3: Query

```csharp
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
        ReadUserQuery query = new(userId);
        Result<UserDTO> result = await _mediator.Send<ReadUserQuery, UserDTO>(
            query, 
            cancellationToken);

        if (result.IsFailure)
        {
            return HandleErrors(result);
        }

        return TypedResults.Ok(result.Value);
    }
}
```

### Pipeline Behavior Integration

The mediator automatically applies registered pipeline behaviors to all requests. This allows cross-cutting concerns (logging, validation, caching, etc.) to be applied automatically without modifying endpoints or handlers.

**Pipeline Flow:**
1. Endpoint calls `mediator.Send()`
2. Mediator finds the appropriate handler
3. Mediator builds a pipeline of registered behaviors
4. Request flows through behaviors (in reverse registration order)
5. Handler processes the request
6. Response flows back through behaviors
7. Result is returned to endpoint

See the [Pipeline Behavior Pattern](#5-pipeline-behavior-pattern) section for more details.

### Benefits
- **Decoupling**: Endpoints don't need to know about specific handlers
- **Single Responsibility**: Each handler handles one request type
- **Testability**: Easy to test handlers and endpoints in isolation
- **Extensibility**: Easy to add new handlers without modifying existing code
- **Cross-cutting Concerns**: Pipeline behaviors apply automatically to all requests
- **Consistency**: All requests follow the same flow through the mediator

### Location
- Mediator Interface: `Application/Abstractions/Messaging/IMediator.cs`
- Mediator Implementation: `Application/Mediator/Mediator.cs`
- Handler Interfaces: `Application/Abstractions/Messaging/`
- Command Handlers: `Application/{Feature}/Commands/`
- Query Handlers: `Application/{Feature}/Queries/`

## 3. Repository Pattern

### Purpose
Abstracts data access logic and provides a more object-oriented view of the persistence layer.

### Implementation

The project uses **separate repositories for commands and queries** (CQRS):

#### Command Repository (Write)
```csharp
public interface ICommandRepository<T> where T : BaseEntity
{
    Task<Result<T>> AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
```

#### Query Repository (Read)
```csharp
public interface IQueryRepository<T> where T : BaseEntity
{
    Task<Result<T>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<T>>> ListAllAsync(CancellationToken cancellationToken = default);
}
```

#### Entity-Specific Repositories
```csharp
// Domain interface
public interface IUserCommandRepository : ICommandRepository<UserEntity> { }
public interface IUserQueryRepository : IQueryRepository<UserEntity> { }

// Infrastructure implementation
public class UserCommandRepository : IUserCommandRepository { }
public class UserQueryRepository : IUserQueryRepository { }
```

### Benefits
- **Abstraction**: Hides data access implementation details
- **Testability**: Easy to mock repositories in tests
- **Flexibility**: Can swap data access implementations
- **Separation**: Keeps domain logic independent of persistence

### Location
- Interfaces: `Domain/Interfaces/`
- Implementations: `Infrastructure/Database/Repositories/`

## 4. Result Pattern

### Purpose
Encapsulates operation results, explicitly representing success or failure without throwing exceptions for business logic errors.

### Implementation

```csharp
// Non-generic result
public sealed class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public IList<Error> Errors { get; }
    
    public static Result Success() => new Result(true, new List<Error>());
    public static Result Failure(Error error) => new Result(false, new List<Error> { error });
}

// Generic result with value
public sealed class Result<TResult>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public IList<Error> Errors { get; }
    public TResult Value { get; }
    
    public static Result<TResult> Success(TResult value) => new Result<TResult>(true, new List<Error>(), value);
    public static Result<TResult> Failure(IList<Error> errors) => new Result<TResult>(false, errors, default!);
}
```

### Error Handling

```csharp
// Error record
public sealed record Error(ErrorCode Code, string Description);

// Error codes
public enum ErrorCode
{
    BadRequest = 400,
    NotFound = 404,
    Conflict = 409,
    InternalServerError = 500,
    // ...
}
```

### Usage Example

```csharp
Result<UserEntity> user = await _repository.GetByIdAsync(id);

if (user.IsFailure)
{
    return Result<UserDTO>.Failure(user.Errors);
}

return Result<UserDTO>.Success(UserMapper.Map(user));
```

### Benefits
- **Explicit error handling**: Errors are part of the return type
- **No exceptions for business logic**: Exceptions reserved for exceptional cases
- **Composable**: Can chain operations easily
- **Type safety**: Compiler enforces error checking

### Location
- `Domain/Common/Result.cs`
- `Domain/Common/Error.cs`
- `Domain/Common/ErrorCode.cs`

## 5. Pipeline Behavior Pattern

### Purpose
Implements cross-cutting concerns (logging, validation, caching) that execute before and after request handling.

### Implementation

```csharp
// Pipeline behavior interface
public interface IPipelineBehavior<TRequest, TResponse>
{
    Task<TResponse> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken,
        Func<Task<TResponse>> next);
}
```

### Logging Pipeline Behavior

```csharp
public class LoggingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken, Func<Task<TResponse>> next)
    {
        // Log request
        _logger.LogInformation("Handling {RequestType} {@Request}", typeof(TRequest).Name, request);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var result = await next();
            stopwatch.Stop();
            
            // Log response
            _logger.LogInformation("Completed {RequestType} ElapsedMs:{ElapsedMs}", 
                typeof(TRequest).Name, stopwatch.ElapsedMilliseconds);
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error handling {RequestType}", typeof(TRequest).Name);
            throw;
        }
    }
}
```

### Registration

```csharp
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehavior<,>));
```

### Benefits
- **Separation of concerns**: Cross-cutting logic separated from business logic
- **Reusability**: Same behavior applies to all requests
- **Composability**: Can chain multiple behaviors
- **Testability**: Easy to test behaviors independently

### Location
- Interface: `Application/Abstractions/PipelineBehaviors/IPipelineBehavior.cs`
- Implementation: `Application/Behaviors/LoggingPipelineBehavior.cs`

## 6. Unit of Work Pattern

### Purpose
Maintains a list of objects affected by a business transaction and coordinates writing out changes and resolving concurrency problems. The implementation includes comprehensive error handling for database exceptions, ensuring that all database errors are properly caught, logged, and converted to domain errors using the Result pattern.

### Implementation

#### Interface

```csharp
// Unit of Work interface
public interface IUnitOfWork
{
    Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

#### Implementation

The `UnitOfWork` implementation provides robust error handling for various database exceptions:

```csharp
public sealed class UnitOfWork(ApplicationDbContext context, ILogger<UnitOfWork> logger) : IUnitOfWork
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<UnitOfWork> _logger = logger;
    
    public async Task<Result> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
        await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict occurred while saving changes to the database.");
            return Result.Failure(new Error(
                ErrorCode.Conflict,
                "The data may have been modified or deleted since it was last read. " +
                "It is recommended to refresh the data and try again."
            ));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update error occurred while saving changes to the database.");
            
            string message = ex.InnerException?.Message ?? ex.Message;
            if (message.Contains("UNIQUE") || message.Contains("duplicate key"))
            {
                return Result.Failure(new Error(
                    ErrorCode.Conflict,
                    "There was a conflict with the database. Please try again."
                ));
            }
            
            return Result.Failure(GenericErrors.SomethingWhenWrong());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while saving changes to the database.");
            return Result.Failure(GenericErrors.SomethingWhenWrong());
    }
}
}
```

### Error Handling

The `UnitOfWork` handles three types of exceptions:

1. **DbUpdateConcurrencyException**: Occurs when a concurrency conflict is detected (e.g., optimistic concurrency violation)
   - Returns a `Conflict` error with a user-friendly message
   - Logged as a warning (expected in concurrent scenarios)

2. **DbUpdateException**: General database update exceptions, including:
   - **Unique constraint violations**: Detected by checking for "UNIQUE" or "duplicate key" in the error message
     - Returns a `Conflict` error
   - **Other database errors**: Returns a generic internal server error
   - All logged as errors

3. **Exception**: Any other unexpected exceptions
   - Returns a generic internal server error
   - Logged as an error

### Usage

Handlers must check the result of `SaveChangesAsync` to handle potential errors:

```csharp
public async Task<Result<UserDTO>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
{
    // ... Code related to this command will be here
    
    // Check the result of SaveChangesAsync
    Result result = await _unitOfWork.SaveChangesAsync(cancellationToken);
    if (result.IsFailure)
    {
        return Result<UserDTO>.Failure(result.Errors);
    }

    return Result<UserDTO>.Success(UserMapper.Map(user));
}
```

### Benefits
- **Transaction management**: Ensures all changes are saved atomically
- **Consistency**: Maintains data consistency across multiple repositories
- **Performance**: Can batch multiple operations
- **Error handling**: Comprehensive exception handling with proper logging
- **Result pattern**: Returns `Result` instead of throwing exceptions, making errors explicit
- **Specific error messages**: Provides meaningful error messages for different failure scenarios
- **Logging**: All exceptions are logged with appropriate log levels

### Error Scenarios

| Scenario | Exception Type | Error Code | Log Level |
|----------|---------------|------------|-----------|
| Successful save | None | Success | N/A |
| Concurrency conflict | `DbUpdateConcurrencyException` | `Conflict` (409) | Warning |
| Unique constraint violation | `DbUpdateException` | `Conflict` (409) | Error |
| Other database errors | `DbUpdateException` | `InternalServerError` (500) | Error |
| Unexpected errors | `Exception` | `InternalServerError` (500) | Error |

### Location
- Interface: `Domain/Interfaces/IUnitOfWork.cs`
- Implementation: `Infrastructure/Database/Common/UnitOfWork.cs`

## 7. Global Exception Handling Pattern

### Purpose
Provides centralized exception handling for all unhandled exceptions in the application, ensuring consistent error responses and proper logging. This complements the Result pattern by handling unexpected exceptions that escape the normal application flow.

### Implementation

The application uses .NET 10's `IExceptionHandler` interface for global exception handling,
implemented in the `GlobalExceptionHandler` class located in WebApi.Exceptions namespace.

### Exception Type Mapping

The handler maps different exception types to appropriate HTTP status codes:

| Exception Type | HTTP Status Code | Error Code |
|---------------|------------------|------------|
| `ArgumentException` / `ArgumentNullException` | 400 Bad Request | `BadRequest` |
| `UnauthorizedAccessException` | 401 Unauthorized | `Unauthorized` |
| `KeyNotFoundException` | 404 Not Found | `NotFound` |
| `NotImplementedException` | 501 Not Implemented | `NotImplemented` |
| Other exceptions | 500 Internal Server Error | `InternalServerError` |

### Environment-Specific Behavior

- **Development Mode**: Returns detailed error information including:
  - Exception message
  - Full stack trace
  - Request path and trace identifier
  
- **Production Mode**: Returns generic error messages to avoid exposing internal implementation details

### Registration

The exception handler is registered in `Program.cs`:

```csharp
// Register the exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Enable exception handling middleware
app.UseExceptionHandler();
```

### Relationship with Result Pattern

The application uses two complementary error handling approaches:

1. **Result Pattern**: For expected business logic errors
   - Validation failures
   - Business rule violations
   - Not found scenarios
   - These are handled explicitly in handlers and endpoints

2. **Global Exception Handler**: For unexpected exceptions
   - System exceptions
   - Unhandled errors
   - Framework-level exceptions
   - These are caught by the global handler

It's not recommended to throw exceptions for business logic errors; instead, use the Result pattern,
however this handler catches unexpected exceptions and some common framework exceptions to ensure consistent error responses.

### Benefits
- **Centralized handling**: All unhandled exceptions handled in one place
- **Consistent responses**: All exceptions return standardized ProblemDetails
- **Security**: Prevents sensitive information leakage in production
- **Logging**: All exceptions logged with full context
- **RFC compliance**: Uses standard ProblemDetails format
- **Environment awareness**: Different behavior for development vs production

### Best Practices
- Use Result pattern for expected business errors
- Let global handler catch unexpected exceptions
- Never expose stack traces in production
- Always log exceptions with context (request path, trace ID)
- Use appropriate HTTP status codes for different exception types

### Location
- Implementation: `WebApi/Exceptions/GlobalExceptionHandler.cs`
- Registration: `WebApi/Program.cs`

## 8. Value Object Pattern

### Purpose
Represents a descriptive aspect of the domain with no conceptual identity. Value objects are immutable and defined by their attributes.

### Implementation

```csharp
// Example: Email value object
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
        // Validation logic
    }
}
```

### Usage in Entities

```csharp
public sealed class UserEntity : BaseEntity
{
    public Email Email { get; private set; }
    public FirstName FirstName { get; private set; }
    public LastName LastName { get; private set; }
    
    public static UserEntity Create(Email email, FirstName firstName, LastName lastName)
    {
        return new UserEntity(email, firstName, lastName);
    }
}
```

### Benefits
- **Immutability**: Value objects cannot be changed after creation
- **Validation**: Validation logic encapsulated in value object
- **Type safety**: Prevents invalid values at compile time
- **Domain modeling**: Better represents domain concepts

### Location
- `Domain/Abstractions/ValueObjects/`
- `Domain/Entities/{Entity}/ValueObjects/`

## 9. Factory Pattern

### Purpose
Provides a way to create objects without specifying the exact class of object that will be created.

### Implementation

Used for entity creation to ensure valid state:

```csharp
public sealed class UserEntity : BaseEntity
{
    private UserEntity(Email email, FirstName firstName, LastName lastName) : base()
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
    }
    
    // Factory method
    public static UserEntity Create(Email email, FirstName firstName, LastName lastName)
    {
        return new UserEntity(email, firstName, lastName);
    }
}
```

### Benefits
- **Encapsulation**: Hides construction complexity
- **Validation**: Ensures entities are created in valid state
- **Immutability**: Private constructor prevents direct instantiation
- **Domain rules**: Enforces business rules during creation

### Location
- Used in all domain entities: `Domain/Entities/`

## Best Practices

1. **One handler per command/query**: Keep handlers focused
2. **Use Result pattern**: Don't throw exceptions for business logic errors
3. **Global exception handler**: Catches unexpected exceptions, complements Result pattern
4. **Immutable value objects**: Once created, value objects don't change
5. **Factory methods**: Use static factory methods for entity creation
6. **Pipeline behaviors**: Use for cross-cutting concerns, not business logic
7. **Unit of Work**: Always use for coordinating multiple repository operations

