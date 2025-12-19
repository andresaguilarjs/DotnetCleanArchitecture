# Clean Architecture Example

A comprehensive implementation of Clean Architecture in C# (.NET 10.0), demonstrating best practices, design patterns, and modern development techniques.

## Overview

This project serves as a practical example of Clean Architecture principles, featuring:

- **Clean Architecture** with clear layer separation (Domain, Application, Infrastructure, WebApi)
- **CQRS Pattern** - Separate command and query handlers
- **Repository Pattern** - Abstracted data access with CQRS separation
- **Result Pattern** - Explicit error handling without exceptions
- **Pipeline Behaviors** - Cross-cutting concerns (logging, validation)
- **Value Objects** - Domain-driven design with validation
- **FastEndpoints** - Modern, performant API framework
- **Entity Framework Core** - Code-first database approach
- **Scalar** - Beautiful API documentation

## Features

- ✅ User CRUD operations (Create, Read, Update, Delete)
- ✅ Soft delete support
- ✅ Health checks
- ✅ Global exception handler (IExceptionHandler)
- ✅ Request/response logging
- ✅ OpenAPI/Scalar documentation
- ✅ Value object validation
- ✅ Unit of Work pattern
- ✅ Domain services

## Requirements

- **.NET SDK 10.0** or later
- **SQL Server** (or Docker for containerized SQL Server)

## Quick Start

### 1. Set Up Database

Using Docker (recommended):
```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
   -p 1433:1433 --name sqlserver \
   -d mcr.microsoft.com/mssql/server:2022-latest
```

### 2. Configure Connection String

Update `src/WebApi/appsettings.json` with your database connection:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=CleanArchitectureDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
  }
}
```

### 3. Apply Migrations

```bash
dotnet ef database update --project src/WebApi/WebApi.csproj
```

### 4. Run the Application

```bash
cd src/WebApi
dotnet run
```

### 5. Access API Documentation

- **Scalar UI**: `https://localhost:5001/scalar/v1` (development mode)
- **OpenAPI JSON**: `https://localhost:5001/openapi/v1.json`

## Project Structure

```
src/
├── Domain/          # Core business logic, entities, value objects
├── Application/     # Use cases, commands, queries, DTOs
├── Infrastructure/  # Data access, external services
└── WebApi/          # API endpoints, presentation layer
```

## Documentation

Comprehensive documentation is available in the `docs/` folder:

- **[Architecture Overview](docs/architecture/overview.md)** - Project overview and architecture principles
- **[Layers](docs/architecture/layers.md)** - Detailed explanation of each layer
- **[Design Patterns](docs/architecture/design_patterns.md)** - Implemented patterns and their usage
- **[Getting Started](docs/usage/getting_started.md)** - Detailed setup instructions
- **[Examples](docs/usage/examples.md)** - Code examples and usage patterns
- **[Extending](docs/usage/extending.md)** - Guide for adding new features

## Architecture Layers

### Domain Layer
- Core business logic
- Entities and value objects
- Domain interfaces
- No dependencies on other layers

### Application Layer
- Use cases (commands and queries)
- DTOs and mappers
- Application services
- Pipeline behaviors
- Depends only on Domain

### Infrastructure Layer
- Entity Framework Core implementation
- Repository implementations
- Database context and migrations
- Health checks
- Depends on Domain and Application

### WebApi Layer
- FastEndpoints
- HTTP request/response handling
- API documentation
- Depends on all layers

## Design Patterns

- **CQRS**: Separate command and query handlers
- **Repository Pattern**: Abstracted data access
- **Mediator Pattern**: Request/response handling
- **Result Pattern**: Explicit error handling
- **Pipeline Behavior**: Cross-cutting concerns
- **Unit of Work**: Transaction management
- **Value Object**: Domain validation
- **Factory Pattern**: Entity creation

## Technologies

- **.NET 10.0**
- **ASP.NET Core**
- **FastEndpoints 7.1.1**
- **Entity Framework Core 10.0**
- **SQL Server**
- **Scalar 2.11.1** (API documentation)

## Development

### Running Tests
```bash
dotnet test
```

### Creating Migrations
```bash
dotnet ef migrations add MigrationName --project src/WebApi/WebApi.csproj
```

### Hot Reload
```bash
dotnet watch run --project src/WebApi/WebApi.csproj
```

## Contributing

This is a learning/example project. Feel free to:
- Fork and experiment
- Submit issues
- Suggest improvements
- Use as a reference for your projects

## License

This project is provided as-is for educational purposes.

## Additional Resources

- [FastEndpoints Documentation](https://fast-endpoints.com/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/ef/core/)
