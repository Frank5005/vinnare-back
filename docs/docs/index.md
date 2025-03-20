# Vinnare eCommerce Project Documentation

## Overview
Vinnare is an eCommerce API built using .NET, structured into multiple layers for maintainability and scalability. It follows a modular approach, separating concerns across different projects.

## Project Structure

- **Api/**: Contains the API layer with controllers, DTOs, middleware, and extensions.
- **Data/**: Manages the database context, entities, and migrations.
- **Services/**: Implements business logic with service classes and interfaces.
- **Shared/**: Contains shared configurations, DTOs, enums, and exceptions.
- **ConsoleAppMigration/**: A console application for managing database migrations.
- **docs/**: Documentation files for the project using MkDocs.
- **.github/**: CI/CD workflows for automated builds and deployments.
- **logs/**: Contains application logs.

## Program.cs Overview

The `Program.cs` file serves as the entry point of the API, configuring essential components:

- **Service Registration**
  - Controllers
  - Swagger for API documentation
  - Security settings (JWT, authentication, authorization)
  - Database configuration using Entity Framework Core
  - Logging configuration

- **Middleware Configuration**
  - Custom middleware for authentication and error handling
  - HTTPS redirection
  - Authentication & Authorization enforcement
  - API routing

## How to Run the Project
To run the project, ensure you are in the **root folder**.

### Using Docker:
```sh
docker build . -f ./Api/Dockerfile
```

### Running Locally:
```sh
dotnet restore
dotnet build
dotnet Api/bin/Debug/net8.0/Api.dll
```

## Documentation Structure
The documentation is managed using MkDocs, with sections covering:
- **Features** (`features.md`)
- **Setup and Execution** (`how_to_run.md`)
- **Code Structure** (`code.md`)
- **CI/CD Processes** (`ci_cd.md`)
- **Technical Issues and Troubleshooting** (`technical_issues.md`)
- **Architecture Diagrams** (`diagrams.md`)
