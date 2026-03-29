# Design Patterns

This document describes the design patterns implemented in this Clean Architecture project.

## Overview

The project implements several design patterns to achieve separation of concerns, maintainability, and testability:

1. **CQRS (Command Query Responsibility Segregation)**
2. **Mediator Pattern**
3. **Repository Pattern**
4. **Result Pattern**
5. **Pipeline Behavior Pattern**
6. **Validation Pattern (Two-Layer Approach)**
7. **Unit of Work Pattern**
8. **Global Exception Handling Pattern**
9. **Value Object Pattern**
10. **Factory Pattern**
11. **Domain Events**

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
The mediator is registered as a Scoped service. The handlers are registered in dependency injection using **Scrutor** for automatic discovery:

```csharp
// Application/DependencyInjection.cs

public static IServiceCollection AddApplication(this IServiceCollection services)
{
// Register mediator
services.AddScoped<IMediator, Application.Mediator.Mediator>();

    // Automatically register all command handlers, query handlers, and pipeline behaviors
    // using Scrutor assembly scanning
    services.Scan(scan => scan.FromAssembliesOf(typeof(DependencyInjection))
        .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime()
        .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime()
        .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime()
        .AddClasses(classes => classes.AssignableTo(typeof(IPipelineBehavior<,>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime()
    );

    return services;
}
```

**Key Points:**
- **Automatic Discovery**: All handlers and pipeline behaviors are automatically discovered and registered
- **No Manual Registration**: You don't need to manually register each handler
- **Supports Internal Classes**: `publicOnly: false` allows registration of `internal` handler classes
- **Scoped Lifetime**: All handlers are registered with scoped lifetime

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

## 6. Validation Pattern (Two-Layer Approach)

### Purpose
Implements a two-layer validation strategy that provides early feedback at the application boundary while maintaining domain integrity through value object validations. This approach ensures invalid data is caught early and domain invariants are always enforced.

### Two-Layer Validation Strategy

The project uses **two complementary validation layers**:

1. **Pipeline Behavior Validation (Application Layer)**
   - Validates input/DTOs before they reach handlers
   - Provides early feedback and fail-fast behavior
   - Focuses on input format and basic constraints
   - Runs in the pipeline before business logic

2. **Domain Object Validation (Domain Layer)**
   - Validates when creating value objects (Email, FirstName, LastName, etc.)
   - Enforces business rules and domain invariants
   - Provides defense in depth
   - Ensures domain integrity regardless of entry point

### Why Both Layers?

**Pipeline Behavior Validation:**
- ✅ Catches invalid input early (before handler execution)
- ✅ Provides immediate feedback to API consumers
- ✅ Reduces unnecessary processing of invalid data
- ✅ Validates DTO/request structure

**Domain Object Validation:**
- ✅ Enforces domain invariants (business rules)
- ✅ Works even if value objects are created from other sources (database, migrations, etc.)
- ✅ Provides defense in depth (catches what pipeline might miss)
- ✅ Ensures domain integrity is always maintained

### Implementation

#### Layer 1: Pipeline Behavior Validation

```csharp
// Application/Abstractions/Validation/IValidator.cs
public interface IValidator<in TRequest>
{
    Task<Result> ValidateAsync(TRequest request, CancellationToken cancellationToken = default);
}

// Application/Behaviors/ValidationPipelineBehavior.cs


// Application/Users/Commands/CreateUser/CreateUserCommandValidator.cs
public class CreateUserCommandValidator : IValidator<CreateUserCommand>
{
    public Task<Result> ValidateAsync(CreateUserCommand request, CancellationToken cancellationToken = default)
    {
        // ... validations

        return errors.Any() 
            ? Task.FromResult(Result.Failure(errors)) 
            : Task.FromResult(Result.Success());
    }
}
```

#### Layer 2: Domain Object Validation

```csharp
// Domain/Entities/User/ValueObjects/Email.cs
public record Email
{
    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result<Email>.Failure(new List<Error> { UserErrors.EmptyEmail() });
        }

        if (!IsValid(email))
        {
            return Result<Email>.Failure(new List<Error> { UserErrors.InvalidEmail() });
        }

        return Result<Email>.Success(new Email(email));
    }
}
```

### Registration

Validators and the validation behavior are registered in `Application/DependencyInjection.cs`:

```csharp
// Register validation behavior (order matters - runs before other behaviors)
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));

// Auto-register all validators
services.Scan(scan => scan.FromAssembliesOf(typeof(DependencyInjection))
    .AddClasses(classes => classes.AssignableTo(typeof(IValidator<>)), publicOnly: false)
        .AsImplementedInterfaces()
        .WithScopedLifetime()
);
```

### Validation Flow

```
1. Request arrives at API endpoint
   ↓
2. Pipeline Behavior Validation (early validation)
   - Validates DTO/request structure
   - Returns errors immediately if invalid
   ↓
3. Handler executes
   ↓
4. Domain Service creates value objects
   ↓
5. Domain Object Validation (domain invariants)
   - Email.Create(), FirstName.Create(), etc.
   - Enforces business rules
   ↓
6. Business logic continues
```

### Benefits

- **Early Feedback**: Invalid input is caught before handler execution
- **Domain Integrity**: Domain validations ensure invariants are always enforced
- **Defense in Depth**: Two layers catch different types of issues
- **Separation of Concerns**: Input validation vs. business rule validation
- **No Exceptions**: Both layers use Result pattern, no exceptions thrown
- **Testability**: Each layer can be tested independently
- **Flexibility**: Domain objects can be created from any source safely

### When to Use Each Layer

**Pipeline Behavior Validation:**
- Input format validation (required fields, basic format)
- DTO structure validation
- Early rejection of obviously invalid data
- API contract validation

**Domain Object Validation:**
- Business rule enforcement (email format, name length, etc.)
- Domain invariants
- Complex validation logic
- Rules that must always be enforced

### Best Practices

1. **Keep Pipeline Validations Light**: Focus on format and structure
2. **Keep Domain Validations Comprehensive**: Enforce all business rules
3. **Don't Duplicate Logic Unnecessarily**: Pipeline can do basic checks, domain does detailed validation
4. **Use Consistent Error Messages**: Both layers can use the same error factory methods
5. **Test Both Layers**: Ensure both validation layers work correctly

### Location
- Validator Interface: `Application/Abstractions/Validation/IValidator.cs`
- Validation Behavior: `Application/Behaviors/ValidationPipelineBehavior.cs`
- Validators: `Application/{Feature}/Commands/{Command}/{Command}Validator.cs`
- Domain Validations: `Domain/Entities/{Entity}/ValueObjects/`

## 7. Unit of Work Pattern

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

The `UnitOfWork` implementation provides robust error handling for various database exceptions.

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

## 8. Global Exception Handling Pattern

### Purpose
Provides centralized exception handling for all unhandled exceptions in the application, ensuring consistent error responses and proper logging. This complements the Result pattern by handling unexpected exceptions that escape the normal application flow.

### Implementation
The application uses .NET 10's `IExceptionHandler` interface for global exception handling,
implemented in the `GlobalExceptionHandler` class located in `WebApi.Exceptions` namespace.

### Exception Type Mapping
The handler maps different exception types to appropriate HTTP status codes. Exceptions are first converted to `Error` objects using `GenericErrors` methods, then to `ProblemDetails` via `ErrorToProblemDetailsConverter`:

| Exception Type | HTTP Status Code | Error Code | GenericErrors Method |
|---------------|------------------|------------|---------------------|
| `ArgumentException` / `ArgumentNullException` | 400 | `ValidationError` | `ArgumentError()` |
| `UnauthorizedAccessException` | 401 | `Unauthorized` | `Unauthorized()` |
| `KeyNotFoundException` | 404 | `NotFound` | `NotFound()` |
| `InvalidOperationException` / `NotSupportedException` | 501 | `NotImplemented` | `NotImplemented()` |
| Other exceptions | 500 | `InternalServerError` | `SomethingWhenWrong()` |

### Implementation Details
The handler delegates the conversion to `ErrorToProblemDetailsConverter.GetProblemDetails()` which handles:
- Converting `Error` objects to `ProblemDetails` format
- Environment-specific behavior (development vs production)
- Setting the `Instance` property to the request path

Response format:
- Content-Type: `"application/problem+json"`
- JSON serialization uses camelCase naming policy
- Returns `false` if HTTP response has already started (prevents writing to a committed response)

### Environment-Specific Behavior
- **Development Mode**: Returns detailed error information including:
  - Exception message
  - Full stack trace
  - Request path (in Instance property) and trace identifier
  
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

## 9. Value Object Pattern

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

## 10. Factory Pattern

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

## 11. Domain Events

### Purpose
Decouple the core use case (command handler) from side effects such as notifications, emails, or integration with other bounded contexts. The handler stays focused on orchestration and returning a result; reactions to “something happened” are implemented separately and delivered through the message broker.

### Contracts vs. implementations
- **Domain** defines the contracts `IDomainEvent` and `IDomainEventHandler<TDomainEvent>`, plus concrete **event types** (records implementing `IDomainEvent`) that are published to the bus—for example `Domain/Entities/User/Events/UserRegisteredDomainEvent.cs` with namespace `Domain.Entities.Users.Events`. `BaseEntity` provides a `DomainEvents` collection plus `RaiseDomainEvent` and `ClearDomainEvents` so aggregates can record what occurred before persistence.
- **Application** defines `IDomainEventHandler<T>` implementations, typically under `Application/{Feature}/Events/`.
- **Infrastructure** configures **MassTransit** (RabbitMQ transport, EF Core **transactional outbox**), publishes from `UnitOfWork`, and hosts **`IConsumer<T>`** classes that resolve the corresponding `IDomainEventHandler<T>` and call `Handle`.

This keeps the dependency rule: Application depends only on Domain; Infrastructure references both for wiring.

### Publishing and outbox
`UnitOfWork.SaveChangesAsync` collects domain events from tracked `BaseEntity` instances, publishes each event via **`IPublishEndpoint.Publish`** (MassTransit stages outbound messages in the **EF outbox**), then calls **`SaveChangesAsync`** on the DbContext so **entity state and outbox rows commit in one transaction**. After a successful save, in-memory `DomainEvents` lists are cleared on tracked entities. A background MassTransit component forwards staged messages to RabbitMQ (`UseBusOutbox`).

Command handlers do not inject `IPublishEndpoint` directly; publishing is centralized in the unit of work.

### Registration
- **Handlers**: In `Application/DependencyInjection.cs`, **Scrutor** scans the Application assembly for classes assignable to `IDomainEventHandler<>` and registers them as scoped (same pattern as command and query handlers).
- **MassTransit**: In `Infrastructure/DependencyInjection.cs`, `AddMassTransit` registers the EF outbox on `ApplicationDbContext`, RabbitMQ, and `AddConsumers` for the Infrastructure assembly so consumers are discovered automatically.

```csharp
// Application/DependencyInjection.cs (excerpt) — handlers only; no dispatcher registration

services.Scan(scan => scan.FromAssembliesOf(typeof(DependencyInjection))
    // ... validators, command handlers, query handlers ...
    .AddClasses(classes => classes.AssignableTo(typeof(IDomainEventHandler<>)), publicOnly: false)
        .AsImplementedInterfaces()
        .WithScopedLifetime()
);
```

### Raising events and when they run
Use cases call `RaiseDomainEvent` on a `BaseEntity` before `IUnitOfWork.SaveChangesAsync`. Map `DomainEvents` as not stored in the database: in each EF entity configuration, use `builder.Ignore(x => x.DomainEvents)`.

Handler code runs **later**, when a consumer processes the message—not in the same call stack as the command handler. That is **eventual consistency** between the database commit and the side effect.

### Example
**Event** (`Domain/Entities/User/Events/UserRegisteredDomainEvent.cs`):

```csharp
namespace Domain.Entities.Users.Events;

public sealed record UserRegisteredDomainEvent : IDomainEvent
{
    public Guid UserId { get; init; }
}
```

**Handler** (`Application/Users/Events/SendWelcomeEmailHandler.cs`) — placeholder that logs a welcome message:

```csharp
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

**Consumer** (`Infrastructure/Messaging/User/SendWelcomeEmailConsumer.cs`) — bridges the message to the handler:

```csharp
public class SendWelcomeEmailConsumer(IDomainEventHandler<UserRegisteredDomainEvent> domainEventHandler)
    : IConsumer<UserRegisteredDomainEvent>
{
    public Task Consume(ConsumeContext<UserRegisteredDomainEvent> context)
        => domainEventHandler.Handle(context.Message, context.CancellationToken);
}
```

**Raising from a use case** (before `SaveChangesAsync`): e.g. `user.Value.RaiseDomainEvent(new UserRegisteredDomainEvent { UserId = user.Value.Id });`

### Reliability and failure handling
The **transactional outbox** avoids losing messages if the API process crashes after the database commits but before a raw publish would have completed: messages are written to SQL Server in the same transaction as domain data. If a **consumer** throws, MassTransit/RabbitMQ retry and error transport behavior apply; the originating transaction is already committed, so design idempotent handlers and compensations as needed.

### Location
- Contracts: `Domain/Interfaces/IDomainEvent.cs`, `IDomainEventHandler.cs`
- Entity support: `Domain/Abstractions/BaseEntity.cs`
- Example event type: `Domain/Entities/User/Events/UserRegisteredDomainEvent.cs`
- Example handler: `Application/Users/Events/SendWelcomeEmailHandler.cs`
- Publish orchestration: `Infrastructure/Database/Common/UnitOfWork.cs`
- MassTransit registration: `Infrastructure/DependencyInjection.cs`
- Example consumer: `Infrastructure/Messaging/User/SendWelcomeEmailConsumer.cs`
- EF: ignore `DomainEvents` in `Infrastructure/Database/Configurations/` (e.g. `UserConfiguration.cs`); outbox/inbox entities registered on `ApplicationDbContext`

## Best Practices

1. **One handler per command/query**: Keep handlers focused
2. **Use Result pattern**: Don't throw exceptions for business logic errors
3. **Global exception handler**: Catches unexpected exceptions, complements Result pattern
4. **Immutable value objects**: Once created, value objects don't change
5. **Factory methods**: Use static factory methods for entity creation
6. **Pipeline behaviors**: Use for cross-cutting concerns, not business logic
7. **Unit of Work**: Always use for coordinating multiple repository operations
8. **Domain events**: Raise on `BaseEntity` before `SaveChanges`; let `UnitOfWork` publish via MassTransit outbox; discover `IDomainEventHandler<T>` via Scrutor; add `IConsumer<T>` in Infrastructure when the event is published to the bus; keep handlers separate from the mediator pipeline

