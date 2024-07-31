# Dotnet Clean Architecture

It is a project with the purpose to practice a Clean Architecture, Design Patterns, Libraries that I considered interesting.

## Requirements

- NET 8.0
- SQL Server

## Instalation

1. Create a new SQL Server Database. I recommend using a Docker image with SQL Server. I am use the following: `mcr.microsoft.com/mssql/server:2022-latest`
2. Update the `appsettings.json` with the database connection. I know that put this here is a security mistake, but for now I keep it here
3. As the migrations are already in the project, run the command `dotnet ef database update --project .\src\WebApi\WebApi.csproj` to create the tables
