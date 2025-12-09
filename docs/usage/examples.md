# Code Examples

This document provides practical code examples demonstrating how to use the Clean Architecture patterns in this project.

## Table of Contents

1. [Creating a Command](#creating-a-command)
2. [Creating a Query](#creating-a-query)
3. [Creating an Endpoint](#creating-an-endpoint)
4. [Working with Value Objects](#working-with-value-objects)
5. [Error Handling](#error-handling)
6. [Using the Result Pattern](#using-the-result-pattern)
7. [Domain Service Example](#domain-service-example)

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
using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;
using Domain.Interfaces;

namespace Application.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, UserDTO>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserCommandRepository _userCommandRepository;
    private readonly IUserService _userService;

    public CreateUserCommandHandler(
        IUnitOfWork unitOfWork, 
        IUserCommandRepository userRepository, 
        IUserService userService)
    {
        _unitOfWork = unitOfWork;
        _userCommandRepository = userRepository;
        _userService = userService;
    }

    public async Task<Result<UserDTO>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Use domain service to create entity (includes validation)
        Result<UserEntity> user = _userService.CreateUserEntity(
            request.UserRequest.Email, 
            request.UserRequest.FirstName, 
            request.UserRequest.LastName);

        if (user.IsFailure)
        {
            return Result<UserDTO>.Failure(user.Errors);
        }

        // Add to repository
        await _userCommandRepository.AddAsync(user);
        
        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map to DTO and return
        return Result<UserDTO>.Success(UserMapper.Map(user));
    }
}
```

### Step 3: Register the Handler

```csharp
// Application/DependencyInjection.cs
services.AddScoped<ICommandHandler<CreateUserCommand, UserDTO>, CreateUserCommandHandler>();
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

### Step 3: Register the Handler

```csharp
// Application/DependencyInjection.cs
services.AddScoped<IQueryHandler<ReadUserQuery, UserDTO>, ReadUserQueryHandler>();
```

## Creating an Endpoint

Endpoints handle HTTP requests and delegate to command/query handlers.

### Step 1: Create the Endpoint

```csharp
// WebApi/Endpoints/Users/CreateUserEndpoint.cs
using Application.Users;
using Application.Users.Commands.CreateUser;
using Domain.Common;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using WebApi.Abstractions;

namespace WebApi.Endpoints.Users;

public class CreateUserEndpoint : BaseEndpoint<UserRequest, Results<Ok<UserDTO>, NotFound, ProblemDetails>, UserDTO>
{
    private readonly Application.Abstractions.Messaging.ICommandHandler<CreateUserCommand, UserDTO> _createUserCommandHandler;

    public CreateUserEndpoint(
        Application.Abstractions.Messaging.ICommandHandler<CreateUserCommand, UserDTO> createUserCommandHandler)
    {
        _createUserCommandHandler = createUserCommandHandler;
    }

    public override void Configure()
    {
        Post("/api/users");
        AllowAnonymous(); // Or use RequireAuthorization() for protected endpoints
        Description(d => d
            .Produces<UserDTO>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(404));
    }

    public override async Task<Results<Ok<UserDTO>, NotFound, ProblemDetails>> ExecuteAsync(
        UserRequest request, 
        CancellationToken cancellationToken)
    {
        CreateUserCommand createUserCommand = new(request);
        Result<UserDTO> result = await _createUserCommandHandler.Handle(createUserCommand, cancellationToken);

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
// Domain/Entities/User/Interfaces/IUserService.cs
using Domain.Common;
using Domain.Entities.User;

namespace Domain.Entities.User.Interfaces;

public interface IUserService
{
    Result<UserEntity> CreateUserEntity(string email, string firstName, string lastName);
}
```

### Step 2: Implement the Domain Service

```csharp
// Application/Users/Services/UserServices.cs
using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;
using Domain.Entities.User.ValueObjects;

namespace Application.Users.Services;

public sealed class UserService : IUserService
{
    public Result<UserEntity> CreateUserEntity(string email, string firstName, string lastName)
    {
        Result<Email> emailResult = Email.Create(email);
        if (emailResult.IsFailure)
        {
            return Result<UserEntity>.Failure(emailResult.Errors);
        }

        Result<FirstName> firstNameResult = FirstName.Create(firstName);
        if (firstNameResult.IsFailure)
        {
            return Result<UserEntity>.Failure(firstNameResult.Errors);
        }

        Result<LastName> lastNameResult = LastName.Create(lastName);
        if (lastNameResult.IsFailure)
        {
            return Result<UserEntity>.Failure(lastNameResult.Errors);
        }

        return Result<UserEntity>.Success(
            UserEntity.Create(emailResult.Value, firstNameResult.Value, lastNameResult.Value)
        );
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
public sealed class UpdateUserCommand : ICommand<UserDTO>
{
    public Guid Id { get; }
    public UserRequest UserRequest { get; }

    public UpdateUserCommand(Guid id, UserRequest userRequest)
    {
        Id = id;
        UserRequest = userRequest;
    }
}
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
    private readonly ICommandHandler<UpdateUserCommand, UserDTO> _updateUserCommandHandler;

    public UpdateUserEndpoint(ICommandHandler<UpdateUserCommand, UserDTO> updateUserCommandHandler)
    {
        _updateUserCommandHandler = updateUserCommandHandler;
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
        Result<UserDTO> result = await _updateUserCommandHandler.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleErrors(result);
        }

        return TypedResults.Ok(result.Value);
    }
}
```

## Best Practices Demonstrated

1. **Separation of Concerns**: Each layer has clear responsibilities
2. **Error Handling**: Use Result pattern, not exceptions for business logic
3. **Validation**: Validate at domain level using value objects
4. **Immutability**: Value objects and entities use private setters
5. **Factory Methods**: Entities created via static factory methods
6. **DTOs**: Domain entities never exposed to API layer
7. **Dependency Injection**: All dependencies injected via constructor

