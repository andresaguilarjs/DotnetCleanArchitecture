# Architecture Layers

This document provides a detailed overview of each layer in the Clean Architecture implementation.

## Layer Overview

The project follows Clean Architecture principles with four distinct layers, each with specific responsibilities and dependency rules:

```
┌─────────────────────────────────────┐
│         WebApi Layer                │  ← Presentation/API
├─────────────────────────────────────┤
│      Application Layer              │  ← Use Cases
├─────────────────────────────────────┤
│      Infrastructure Layer           │  ← External Concerns
├─────────────────────────────────────┤
│         Domain Layer                │  ← Core Business Logic
└─────────────────────────────────────┘
```

## Domain Layer (`src/Domain`)

**Purpose**: Contains the core business logic and entities. This is the innermost layer with no dependencies on other layers.

### Key Components

#### Entities
- **BaseEntity**: Abstract base class for all entities with common properties:
  - `Id` (Guid)
  - `CreatedAt` (DateTime)
  - `LastUpdatedAt` (DateTime)
  - `IsDeleted` (bool) - for soft delete support
  - `Delete()` - abstract method for entity-specific deletion logic

- **UserEntity**: User domain entity with value objects:
  - Email (Value Object)
  - FirstName (Value Object)
  - LastName (Value Object)
  - Factory method: `Create()`
  - Update method with automatic timestamp refresh

#### Value Objects
Located in `Domain/Abstractions/ValueObjects/` and entity-specific `ValueObjects/` folders:
- **Name**: Generic name value object
- **Email**: User email with validation
- **FirstName**: User first name
- **LastName**: User last name

#### Common Types
- **Result<T>**: Generic result type for operations that can succeed or fail
- **Result**: Non-generic result type
- **Error**: Error record with ErrorCode and Description
- **ErrorCode**: Enumeration of HTTP status codes and error types

#### Interfaces
- **ICommandRepository<T>**: Interface for write operations
  - `AddAsync(T entity)`
  - `Update(T entity)`
  - `DeleteAsync(Guid id)`

- **IQueryRepository<T>**: Interface for read operations
  - `GetByIdAsync(Guid id)`
  - `ListAllAsync()`

- **IUnitOfWork**: Transaction management
  - `SaveChangesAsync(CancellationToken)`

- **Entity-specific interfaces**: Located in `Domain/Entities/{Entity}/Interfaces/`
  - Example: `IUserService`, `IUserCommandRepository`, `IUserQueryRepository`

### Design Principles
- No dependencies on other layers
- Pure business logic
- Immutable value objects where appropriate
- Factory methods for entity creation
- Domain services for complex business rules

## Application Layer (`src/Application`)

**Purpose**: Contains application-specific logic, use cases, DTOs, and orchestrates domain operations.

### Key Components

#### Commands (Write Operations)
Located in `Application/{Feature}/Commands/`:
- **CreateUserCommand**: Creates a new user
- **UpdateUserCommand**: Updates an existing user
- **DeleteUserCommand**: Deletes a user (soft delete)

Each command includes:
- Command class (implements `ICommand<TResult>` or `ICommand`)
- Command handler (implements `ICommandHandler<TCommand, TResult>`)

#### Queries (Read Operations)
Located in `Application/{Feature}/Queries/`:
- **ReadUserQuery**: Retrieves a single user by ID
- **ReadUserListQuery**: Retrieves a list of all users

Each query includes:
- Query class (implements `IQuery<TResponse>`)
- Query handler (implements `IQueryHandler<TQuery, TResponse>`)

#### DTOs (Data Transfer Objects)
- **UserDTO**: Data transfer object for User entity
- Located in `Application/{Feature}/` folders

#### Mappers
- **UserMapper**: Maps between `UserEntity` and `UserDTO`
- Converts domain entities to DTOs for API responses

#### Services
- **UserService**: Application service for user-related operations
- Implements domain interfaces like `IUserService`
- Contains business logic that spans multiple entities

#### Pipeline Behaviors
Located in `Application/Behaviors/`:
- **LoggingPipelineBehavior**: Logs all requests and responses
  - Tracks trace IDs
  - Measures execution time
  - Handles cancellation and exceptions

#### Middleware
Located in `Application/Middlewares/`:
- **ExceptionHanddlerMiddleware**: Global exception handler
  - Catches unhandled exceptions
  - Returns standardized error responses
  - Logs errors

#### Abstractions
- **IBaseRequest**: Base interface for all requests
- **ICommand**: Marker interface for commands
- **ICommand<TResult>**: Command with return value
- **IQuery<TResponse>**: Query interface
- **ICommandHandler<TCommand, TResult>**: Command handler interface
- **IQueryHandler<TQuery, TResponse>**: Query handler interface
- **IPipelineBehavior<TRequest, TResponse>**: Pipeline behavior interface

### Design Principles
- Depends only on Domain layer
- Thin layer that orchestrates domain operations
- DTOs for data transfer (no domain entities exposed)
- Handlers contain minimal logic, delegate to domain services
- Pipeline behaviors for cross-cutting concerns

## Infrastructure Layer (`src/Infrastructure`)

**Purpose**: Implements external concerns like database access, file systems, and third-party services.

### Key Components

#### Database Context
- **ApplicationDbContext**: Entity Framework Core DbContext
  - Configures entity mappings
  - Manages database connections
  - Handles migrations

#### Repositories
Located in `Infrastructure/Database/Repositories/`:

**Command Repositories** (`Command/`):
- **UserCommandRepository**: Implements `IUserCommandRepository`
- Generic command repository base classes
- Handles write operations (Add, Update, Delete)

**Query Repositories** (`Query/`):
- **UserQueryRepository**: Implements `IUserQueryRepository`
- Generic query repository base classes
- Handles read operations (GetById, ListAll)

#### Unit of Work
- **UnitOfWork**: Implements `IUnitOfWork`
- Manages database transactions
- Coordinates SaveChanges across repositories

#### Entity Configurations
Located in `Infrastructure/Database/Configurations/`:
- Entity Framework Core configuration classes
- Maps domain entities to database tables
- Configures relationships and constraints

#### Health Checks
Located in `Infrastructure/HealthChecks/`:
- **SqlHealthCheck**: Monitors SQL Server database connectivity
- Registered with ASP.NET Core health checks

#### Migrations
Located in `Infrastructure/Migrations/`:
- Entity Framework Core migrations
- Database schema versioning
- Includes initial migration and soft delete support migration

### Design Principles
- Implements interfaces defined in Domain layer
- Handles all external dependencies
- Database-specific logic isolated here
- Health checks for monitoring
- Migration management

## WebApi Layer (`src/WebApi`)

**Purpose**: Handles HTTP requests, routing, and API documentation. This is the outermost layer.

### Key Components

#### Endpoints
Located in `WebApi/Endpoints/{Feature}/`:
- **CreateUserEndpoint**: POST `/api/users`
- **UpdateUserEndpoint**: PUT `/api/users/{id}`
- **DeleteUserEndpoint**: DELETE `/api/users/{id}`
- **GetUserEndpoint**: GET `/api/users/{id}`
- **GetUsersEndpoint**: GET `/api/users`

All endpoints:
- Inherit from `BaseEndpoint<TRequest, TResponse, TResult>`
- Use FastEndpoints framework
- Handle request/response mapping
- Convert Result types to HTTP responses
- Handle errors via `HandleErrors()` method

#### Base Endpoint
- **BaseEndpoint**: Abstract base class for all endpoints
  - Provides `HandleErrors()` for Result types
  - Converts domain errors to HTTP ProblemDetails
  - Maps ErrorCode to HTTP status codes

#### Request Models
- **UserRequest**: Request model for user operations
- Located in `Application/{Feature}/` folders

#### Dependency Injection
- **DependencyInjection**: Registers presentation services
- Adds controllers (if using traditional MVC)
- Configures FastEndpoints

#### Program.cs
- Application entry point
- Configures services from all layers
- Sets up middleware pipeline
- Configures OpenAPI/Scalar documentation
- Maps health check endpoints

### API Documentation
- **OpenAPI**: Configured via `Microsoft.AspNetCore.OpenApi`
- **Scalar**: Modern API documentation UI (replaces Swagger)
  - Accessible at `/scalar/v1` in development
  - Deep Space theme configured
  - JavaScript HTTP client enabled

### Design Principles
- Thin layer focused on HTTP concerns
- Delegates business logic to Application layer
- Uses FastEndpoints for endpoint definition
- Standardized error responses
- API documentation via Scalar

## Dependency Flow

The dependency rule ensures that:
1. **Domain** has no dependencies
2. **Application** depends only on **Domain**
3. **Infrastructure** depends on **Domain** and **Application**
4. **WebApi** depends on all layers

This ensures that:
- Business logic is independent of frameworks
- Changes to external dependencies don't affect domain logic
- Testing is easier (can mock infrastructure)
- Technology choices can change without affecting core logic

## Layer Communication

### Request Flow
```
HTTP Request → WebApi Endpoint → Application Handler → Domain Service → Infrastructure Repository → Database
```

### Response Flow
```
Database → Infrastructure Repository → Domain Entity → Application DTO → WebApi Endpoint → HTTP Response
```

### Error Flow
```
Any Layer → Result<T>.Failure → Application Handler → WebApi Endpoint → ProblemDetails → HTTP Error Response
```

## Best Practices

1. **Domain Layer**:
   - Keep pure business logic
   - Use value objects for validation
   - Factory methods for entity creation
   - No framework dependencies

2. **Application Layer**:
   - Thin orchestration layer
   - Use DTOs, not domain entities
   - Pipeline behaviors for cross-cutting concerns
   - One handler per command/query

3. **Infrastructure Layer**:
   - Implement domain interfaces
   - Handle all external concerns
   - Keep EF Core specifics here
   - Health checks for monitoring

4. **WebApi Layer**:
   - HTTP-specific concerns only
   - Use FastEndpoints for endpoints
   - Standardized error handling
   - API documentation

