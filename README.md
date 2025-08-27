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
Configure the OneIDM API endpoint and token:
```json
{
  "OneIdm": {
    "ApiUrl": "https://your-oneidm-api.com/userroles",
    "ApiToken": "your-api-token-here"
  }
}
```

## Database Setup

Run the SQL script in `Scripts/CreateUserRolesTable.sql` to create the required table:

```sql
-- Creates UserRoles table with indexes for optimal performance
```

## Architecture

### Key Components

1. **UserRole Entity** (`Entities/UserRole.cs`)
   - Represents the data structure with DomainId, Role, Module fields

2. **Repository Layer** (`Repositories/UserRoleRepository.cs`)
   - Implements data access using Dapper
   - Provides CRUD operations and bulk insert functionality

3. **Service Layer** (`Services/UserRoleSyncService.cs`)
   - Handles business logic for synchronization
   - Fetches data from OneIDM API and transforms it

4. **Worker Service** (`Worker.cs`)
   - Background service that runs the sync every 6 hours
   - Uses service scope factory for proper dependency injection

### Data Flow

1. Worker service starts and schedules sync every 6 hours
2. UserRoleSyncService fetches data from OneIDM API
3. API response is transformed into UserRole entities
4. Existing data is cleared and new data is bulk inserted
5. Success/failure is logged appropriately

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

### Dependencies
- Dapper 2.1.35 - for data access
- System.Data.SqlClient 4.8.6 - for SQL Server connectivity
- Newtonsoft.Json 13.0.3 - for JSON parsing