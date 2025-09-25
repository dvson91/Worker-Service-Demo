# UserRole Synchronization Service

This Worker Service implements automatic synchronization of user roles from the OneIDM system to a local SQL Server database.

## Features

- **Automatic Sync**: Runs every 6 hours to fetch user role data from OneIDM API
- **Database Storage**: Uses Dapper ORM to store data in UserRoles table
- **Clean Architecture**: Implements SOLID principles with repository pattern
- **Error Handling**: Robust error handling with logging
- **Configurable**: Connection strings and API endpoints configurable via appsettings

## Configuration

### Connection String
Update the connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your SQL Server connection string here"
  }
}
```

### OneIDM API Configuration
Configure the OneIDM API settings including automatic Bearer token generation:
```json
{
  "OneIdm": {
    "ApiUrl": "https://your-oneidm-api.com/userroles",
    "TokenEndpoint": "https://your-oneidm-api.com/auth/token",
    "ApiKey": "your-api-key-here",
    "TokenExpirationMinutes": 60,
    "RefreshTokenBeforeExpiryMinutes": 5
  }
}
```

#### Authentication Flow
The service automatically handles Bearer token generation:
1. **API Key Exchange**: Uses the configured `ApiKey` to request Bearer tokens from the `TokenEndpoint`
2. **Automatic Refresh**: Refreshes tokens before they expire (5 minutes before expiry by default)
3. **Retry Logic**: Automatically retries API calls with new tokens if unauthorized (401) responses are received
4. **Token Caching**: Caches valid tokens to minimize authentication requests

## Database Setup

Run the SQL script in `Scripts/CreateUserRolesTable.sql` to create the required table:

```sql
-- Creates UserRoles table with indexes for optimal performance
```

## Architecture

### Key Components

1. **UserRole Entity** (`Entities/UserRole.cs`)
   - Represents the data structure with DomainId, Role, Module fields

2. **Data Access Layer**
   - **DapperContext** (`Data/DapperContext.cs`) - Handles database connections and transaction management
   - **UserRoleRepository** (`Repositories/UserRoleRepository.cs`) - Implements data access using Dapper with CRUD operations and bulk insert functionality

3. **HTTP Communication Layer**
   - **Refit API Clients** (`Clients/IOneIdmApiClient.cs`, `Clients/IOneIdmTokenClient.cs`) - Type-safe HTTP API interfaces
   - **Token Manager** (`Services/OneIdmTokenManager.cs`) - Handles Bearer token lifecycle and automatic refresh

4. **Service Layer** (`Services/UserRoleSyncService.cs`)
   - Handles business logic for synchronization
   - Integrates with Refit clients to fetch data from OneIDM API and transforms it

5. **Worker Service** (`Worker.cs`)
   - Background service that runs the sync every 6 hours
   - Uses service scope factory for proper dependency injection

### Data Flow

1. Worker service starts and schedules sync every 6 hours
2. UserRoleSyncService uses Refit clients to authenticate and fetch data from OneIDM API
3. Token Manager automatically handles Bearer token requests and refresh cycles
4. API response is transformed into UserRole entities
5. Data replacement is performed atomically within a database transaction (truncate + insert)
6. Success/failure is logged appropriately

### Data Consistency

The synchronization process now uses database transactions to ensure data consistency:
- All data operations (truncate and bulk insert) are performed within a single transaction
- If any part of the sync fails, the entire transaction is rolled back
- This prevents partial data states and ensures data integrity

## API Data Structure Expected

The OneIDM API should return data in this format:
```json
{
  "isSuccess": true,
  "message": "Success",
  "statusCode": 200,
  "modules": [
    {
      "moduleName": "Finance",
      "roles": [
        {
          "roleName": "Manager",
          "users": [
            {
              "domainId": "user123",
              "userName": "John Doe"
            }
          ]
        }
      ]
    }
  ]
}
```

## Development

### Building the Project
```bash
dotnet build
```

### Running the Service
```bash
dotnet run
```

## HTTP API Communication

This project uses **Refit** for type-safe HTTP API communication instead of manual HttpClient handling. Refit provides a declarative approach to REST API consumption with automatic serialization, deserialization, and robust error handling.

### Refit Implementation

#### API Client Interfaces

The service defines clean, type-safe interfaces for external API communication:

```csharp
// Token client for authentication
public interface IOneIdmTokenClient
{
    [Post("/token")]
    Task<TokenResponse> GetTokenAsync([Body] TokenRequest request, CancellationToken cancellationToken = default);
}

// API client for user role data
public interface IOneIdmApiClient
{
    [Get("/api/userroles")]
    [Headers("Content-Type: application/json")]
    Task<OneIdmApiResponse> GetUserRolesAsync([Header("Authorization")] string authorization, CancellationToken cancellationToken = default);
}
```

#### Service Registration

Refit clients are registered with HttpClientFactory for optimal connection management:

```csharp
// Refit API Clients with HttpClientFactory
services.AddRefitClient<IOneIdmTokenClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(tokenBaseUri));

services.AddRefitClient<IOneIdmApiClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseUri));
```

#### Usage in Services

Clean dependency injection and type-safe API calls:

```csharp
public class UserRoleSyncService : IUserRoleSyncService
{
    private readonly IOneIdmApiClient _apiClient;
    private readonly ITokenManager _tokenManager;

    public async Task<List<OneIdmModule>?> FetchUserRolesFromApiAsync(CancellationToken cancellationToken)
    {
        var bearerToken = await _tokenManager.GetValidTokenAsync(cancellationToken);
        var authorizationHeader = $"Bearer {bearerToken}";
        
        var response = await _apiClient.GetUserRolesAsync(authorizationHeader, cancellationToken);
        return response?.IsSuccess == true ? response.Modules : null;
    }
}
```

### Refit vs Manual HttpClient Approach

| Aspect | Refit | Manual HttpClient |
|--------|--------|------------------|
| **Code Verbosity** | ✅ Minimal - Interface definitions only | ❌ Verbose - Manual request/response handling |
| **Type Safety** | ✅ Strong typing with compile-time validation | ⚠️ Runtime errors with string-based URLs |
| **Serialization** | ✅ Automatic JSON serialization/deserialization | ❌ Manual serialization with JsonSerializer/Newtonsoft |
| **Error Handling** | ✅ Structured exceptions with detailed context | ❌ Manual HttpStatusCode checking |
| **Testing** | ✅ Easy to mock interface for unit tests | ⚠️ Requires HttpClient mocking frameworks |
| **Maintenance** | ✅ Changes in one interface method | ❌ Update multiple service methods |
| **HttpClientFactory** | ✅ Built-in integration and connection pooling | ⚠️ Manual HttpClientFactory configuration |
| **Authentication** | ✅ Declarative header/authentication attributes | ❌ Manual header management |
| **Request/Response Logging** | ✅ Built-in with DelegatingHandler support | ❌ Custom logging implementation needed |

### Benefits of Using Refit

1. **Reduced Boilerplate**: Eliminates repetitive HttpClient setup, request creation, and response parsing
2. **Improved Maintainability**: API changes only require interface updates
3. **Better Testability**: Easy to create mock implementations for unit testing
4. **Type Safety**: Compile-time validation prevents runtime API errors
5. **Performance**: Integrates seamlessly with HttpClientFactory for connection pooling
6. **Error Handling**: Automatic mapping of HTTP status codes to typed exceptions

### Dependencies
- Dapper 2.1.35 - for data access
- System.Data.SqlClient 4.8.6 - for SQL Server connectivity
- Newtonsoft.Json 13.0.3 - for JSON parsing
- **Refit 8.0.0** - for type-safe HTTP API clients
- **Refit.HttpClientFactory 8.0.0** - for HttpClientFactory integration