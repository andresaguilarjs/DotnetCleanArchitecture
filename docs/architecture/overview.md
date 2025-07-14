# Overview

## Project Introduction
**Project Name**: Clean Architecture

This project is an implementation of Clean Architecture in C#, that included demostrations of various design patterns and best practices in software development.

### Key Features
- Modular and maintainable codebase
- Separation of concerns
- Testable and scalable architecture

## Architecture Overview
### Clean Architecture
Clean Architecture is a software design philosophy that emphasizes the separation of concerns, making the system easier to maintain and test. The core idea is to keep the business logic independent of external frameworks and technologies.

### Layered Structure
- **Domain Layer**: Contains the core business logic and entities.
- **Application Layer**: Manages application-specific logic and use cases.
- **Infrastructure Layer**: Handles external dependencies like databases and APIs.
- **Presentation Layer**: Manages the user interface and presentation logic.
- **WebApi**: Startup Project

## Design Patterns
This project implements several design patterns, including:
- **Repository Pattern**: For data access abstraction.
- **Mediator Pattern**: For handling requests and responses.
- **Result pattern**: For encapsulate responses separating success from failure without throwing exceptions.

## Technologies and Tools
- **Languages**: C#
- **Frameworks**: .NET Core, ASP.NET Core
- **Libraries**: Entity Framework Core, EntityFramework
- **Tools**: Visual Studio Code, Swagger

## Getting Started
### Prerequisites
- .NET SDK 8.0 or later
- SQL Server database