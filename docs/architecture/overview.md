# Overview

## Project Introduction
**Project Name**: Clean Architecture Example

This project is an implementation of Clean Architecture in C#, demonstrating various design patterns and best practices in software development. It serves as a practical example of building maintainable, testable, and scalable applications using Clean Architecture principles.

### Key Features
- **Modular and maintainable codebase**: Clear separation of concerns across layers
- **CQRS Pattern**: Separation of commands (write operations) and queries (read operations)
- **Testable architecture**: Domain logic is independent of external dependencies
- **Scalable design**: Easy to extend with new features and entities
- **Error handling**: Result pattern for explicit error handling without exceptions
- **Pipeline behaviors**: Cross-cutting concerns like logging handled via pipeline behaviors
- **Health checks**: Built-in database health monitoring
- **Soft delete support**: Entities support soft deletion with audit trails

## Architecture Overview
### Clean Architecture
Clean Architecture is a software design philosophy that emphasizes the separation of concerns, making the system easier to maintain and test. The core idea is to keep the business logic independent of external frameworks and technologies.

The architecture follows the dependency rule: dependencies point inward, with the Domain layer at the center having no dependencies on other layers.

### Layered Structure
The project is organized into four main layers:

1. **Domain Layer** (`src/Domain`): Contains the core business logic, entities, value objects, and domain interfaces. This layer has no dependencies on other layers.
2. **Application Layer** (`src/Application`): Contains application-specific logic, use cases (commands and queries), DTOs, and application services. Depends only on the Domain layer.
3. **Infrastructure Layer** (`src/Infrastructure`): Handles external dependencies like database access (Entity Framework Core), repositories implementation, and health checks. Depends on Domain and Application layers.
4. **WebApi Layer** (`src/WebApi`): The presentation layer that handles HTTP requests, endpoints (using FastEndpoints), and API documentation. Depends on all other layers.

## Design Patterns
This project implements several design patterns:

- **CQRS (Command Query Responsibility Segregation)**: Separate interfaces and implementations for commands (write) and queries (read)
- **Repository Pattern**: Abstraction for data access with `ICommandRepository<T>` and `IQueryRepository<T>`
- **Mediator Pattern**: Custom implementation using `ICommandHandler` and `IQueryHandler` for request/response handling
- **Result Pattern**: Encapsulates responses, separating success from failure without throwing exceptions
- **Pipeline Behavior Pattern**: Cross-cutting concerns (logging, validation) handled via pipeline behaviors
- **Unit of Work Pattern**: Transaction management via `IUnitOfWork` interface
- **Value Object Pattern**: Immutable value objects for domain concepts (Email, FirstName, LastName, etc.)

## Technologies and Tools
- **Language**: C# 12
- **Framework**: .NET 10.0
- **Web Framework**: ASP.NET Core
- **API Framework**: FastEndpoints 7.1.1
- **ORM**: Entity Framework Core 10.0
- **Database**: SQL Server
- **API Documentation**: Scalar 2.11.1 (OpenAPI/Swagger alternative)
- **Health Checks**: Microsoft.Extensions.Diagnostics.HealthChecks
- **Dependency Injection**: Built-in Microsoft.Extensions.DependencyInjection

## Project Structure
```
src/
├── Domain/          # Core business logic and entities
├── Application/     # Application use cases and DTOs
├── Infrastructure/  # Data access and external services
└── WebApi/          # API endpoints and presentation
```

## Getting Started
### Prerequisites
- .NET SDK 10.0 or later
- SQL Server database (or Docker container)
- Visual Studio Code, Visual Studio, or Rider IDE

For detailed setup instructions, see [Getting Started Guide](../usage/getting_started.md).