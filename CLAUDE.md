# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

### Building the Solution
```bash
# Build all projects
dotnet build

# Build specific configuration
dotnet build -c Release

# Build specific project
dotnet build SectigoCertificateManager/SectigoCertificateManager.csproj
```

### Running Tests
```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test project
dotnet test SectigoCertificateManager.Tests/SectigoCertificateManager.Tests.csproj

# Run PowerShell Pester tests
Invoke-Pester Module/Tests/*.Tests.ps1
```

### PowerShell Module Build
```powershell
# Build the PowerShell module
powershell -File Module/Build/Build-Module.ps1

# Import the module for testing
Import-Module ./Module/Lib/Default/SectigoCertificateManager.PowerShell.dll
```

### Package Publishing
```powershell
# Build NuGet packages
powershell -File Build/BuildPackage.ps1

# Publish to NuGet
powershell -File Build/PublishPackageNuget.ps1

# Publish to GitHub
powershell -File Build/PublishPackageGitHub.ps1
```

## Architecture Overview

### Core Library Structure
The solution implements a client library for the Sectigo Certificate Manager API with the following key components:

1. **SectigoCertificateManager** (Core Library)
   - `ApiConfig` & `ApiConfigBuilder`: Fluent API for configuration
   - `SectigoClient`: Main HTTP client wrapper implementing `ISectigoClient`
   - `Clients/`: Specialized API clients for different endpoints
     - `CertificatesClient`: Certificate operations
     - `OrdersClient`: Order management
     - `ProfilesClient`: SSL profile management
     - `OrganizationsClient`: Organization management
     - `UsersClient`: User management
     - `CertificateTypesClient`: Certificate type queries
     - `InventoryClient`: Certificate inventory operations
     - `OrderStatusClient`: Order status monitoring

2. **Request/Response Models**
   - `Models/`: Domain entities (Certificate, Order, Profile, etc.)
   - `Requests/`: API request DTOs with builders for complex requests
   - `Responses/`: API response DTOs

3. **PowerShell Module** (SectigoCertificateManager.PowerShell)
   - Cmdlets following PowerShell naming conventions (Get-Sectigo*, New-Sectigo*, etc.)
   - `AsyncPSCmdlet` base class for async operations
   - Examples in `Examples/` directory

### API Versioning
- Default API version: 25.6 (defined in `ApiConfigBuilder`)
- Supports versions 25.5 and 25.6 via `ApiVersion` enum
- Version-specific logic handled in `ApiVersionHelper`

### Authentication & Security
- Basic authentication with username/password
- Bearer token support with automatic refresh capability
- Client certificate support for mutual TLS
- Concurrency limiting via semaphore throttling

### Error Handling
- `ApiException`: Wraps API errors with detailed information
- `AuthenticationException`: Authentication-specific errors
- `ValidationException`: Client-side validation errors
- `ApiErrorHandler`: Centralized error response handling

### Testing Infrastructure
- xUnit tests for C# components
- WireMock.Net for API mocking
- Pester tests for PowerShell cmdlets
- Multi-targeting: net472, net8.0, net9.0

### Key Design Patterns
- Fluent builder pattern for configuration and complex requests
- Async/await throughout for non-blocking operations
- Streaming support for large result sets (certificates, orders)
- Progress reporting for long-running operations
- Disposable pattern for proper resource cleanup