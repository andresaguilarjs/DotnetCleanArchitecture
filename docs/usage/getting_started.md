# Getting Started

This guide will help you set up and run the Clean Architecture Example project.

## Prerequisites

Before you begin, ensure you have the following installed:

- **.NET SDK 10.0** or later ([Download](https://dotnet.microsoft.com/download))
- **SQL Server** database (or Docker for running SQL Server in a container)
- **IDE** (Visual Studio, Visual Studio Code, or Rider)
- **Git** (optional, for cloning the repository)

## Installation Steps

### 1. Clone or Download the Project

If using Git:
```bash
git clone <repository-url>
cd CleanArchitectureExample
```

Or download and extract the project to your desired location.

### 2. Set Up SQL Server Database

You have two options for running SQL Server:

#### Option A: Docker (Recommended)

Run SQL Server in a Docker container:

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
   -p 1433:1433 --name sqlserver \
   -d mcr.microsoft.com/mssql/server:2022-latest
```

**Note**: Replace `YourStrong@Passw0rd` with a strong password. The password must meet SQL Server password requirements (at least 8 characters, including uppercase, lowercase, numbers, and special characters).

#### Option B: Local SQL Server Installation

Install SQL Server locally and create a new database named `CleanArchitectureDb` (or your preferred name).

### 3. Configure Database Connection

Update the connection string in `src/WebApi/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=CleanArchitectureDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
  }
}
```

**For Docker**: Use the connection string above (adjust password if different).

**For Local SQL Server**: Update the connection string to match your local setup:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CleanArchitectureDb;Integrated Security=true;TrustServerCertificate=True;"
  }
}
```

**Security Note**: In production, store connection strings in secure configuration (Azure Key Vault, environment variables, etc.). The current setup is for development only.

### 4. Apply Database Migrations

The project includes Entity Framework Core migrations. Apply them to create the database schema:

```bash
cd src/WebApi
dotnet ef database update --project ../Infrastructure/Infrastructure.csproj
```

Or from the project root:

```bash
dotnet ef database update --project src/WebApi/WebApi.csproj
```

This will:
- Create the database if it doesn't exist
- Apply all migrations to create tables (Users, Posts, Comments)

### 5. Build the Project

Build the solution to ensure everything compiles:

```bash
dotnet build
```

Or build a specific project:

```bash
dotnet build src/WebApi/WebApi.csproj
```

### 6. Run the Application

Start the Web API:

```bash
cd src/WebApi
dotnet run
```

Or run from the solution root:

```bash
dotnet run --project src/WebApi/WebApi.csproj
```

The API will start on:
- **HTTP**: `http://localhost:5000` (or the port configured in `launchSettings.json`)
- **HTTPS**: `https://localhost:5001` (or the port configured in `launchSettings.json`)

### 7. Access API Documentation

Once the application is running, access the API documentation:

- **Scalar UI**: `https://localhost:5001/scalar/v1` (in development mode)
- **OpenAPI JSON**: `https://localhost:5001/openapi/v1.json`

Scalar provides an interactive API documentation interface where you can:
- View all available endpoints
- Test API calls directly
- See request/response schemas

## Verify Installation

### Test the API

You can test the API using:

1. **Scalar UI**: Navigate to the Scalar documentation page and use the interactive interface
2. **Postman/Insomnia**: Import the OpenAPI specification
3. **curl**: Use command-line tools

#### Example: Create a User

```bash
curl -X POST "https://localhost:5001/api/users" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@example.com",
    "firstName": "John",
    "lastName": "Doe"
  }'
```

#### Example: Get All Users

```bash
curl -X GET "https://localhost:5001/api/users"
```

#### Example: Get User by ID

```bash
curl -X GET "https://localhost:5001/api/users/{userId}"
```

## Project Structure

After setup, your project structure should look like:

```
CleanArchitectureExample/
├── src/
│   ├── Domain/          # Core business logic
│   ├── Application/     # Application use cases
│   ├── Infrastructure/   # Data access & external services
│   └── WebApi/          # API endpoints
├── docs/                # Documentation
├── docker/              # Docker configuration

```

## Common Issues and Solutions

### Issiue: Database Connecton Failed

**Symptoms**: Error message about database connection

**Solutions**:
1. Verify SQL Server is running: `docker ps` (for Docker) or check SQL Server services
2. Check connection string in `appsettings.json`
3. Ensure firewall allows connections on port 1433
4. For Docker: Verify container is running: `docker ps -a`

### Issue: Migrations Not Found

**Symptoms**: Error about migrations not being found

**Solutions**:
1. Ensure you're running the command from the correct directory
2. Verify migrations exist in `src/Infrastructure/Migrations/`
3. Use the full project path: `dotnet ef database update --project src/WebApi/WebApi.csproj`

### Issue: Port Already in Use

**Symptoms**: Error about port being already in use

**Solutions**:
1. Change the port in `src/WebApi/Properties/launchSettings.json`
2. Stop other applications using the same port
3. Use a different port: `dotnet run --urls "http://localhost:5002"`

### Issue: SSL Certificate Errors

**Symptoms**: SSL certificate validation errors

**Solutions**:
1. Trust the development certificate: `dotnet dev-certs https --trust`
2. Use HTTP instead of HTTPS for testing
3. Add `TrustServerCertificate=True` to connection string (already included)

## Next Steps

Now that you have the project running:

1. **Explore the API**: Use Scalar UI to explore available endpoints
2. **Read the Documentation**: 
   - [Architecture Overview](../architecture/overview.md)
   - [Layers](../architecture/layers.md)
   - [Design Patterns](../architecture/design_patterns.md)
3. **Review Examples**: See [Examples](./examples.md) for code samples
4. **Extend the Project**: Follow [Extending Guide](./extending.md) to add new features

## Development Workflow

### Running Tests

```bash
dotnet test
```

### Creating New Migrations

After modifying entities:

```bash
cd src/WebApi
dotnet ef migrations add MigrationName --project ../Infrastructure/Infrastructure.csproj
```

### Updating Database Schema

```bash
dotnet ef database update --project src/WebApi/WebApi.csproj
```

## Additional Resources

- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [FastEndpoints Documentation](https://fast-endpoints.com/)

