# Worker Service Demo

Worker Service Demo is a .NET 9.0 worker service application demonstrating HTTP client patterns, dependency injection, and background service implementation. The application includes services for HTTP communication with authentication handlers, response processors, and extensible service registration.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### Prerequisites and Setup
- **CRITICAL**: This project requires .NET 9.0 SDK. The default system SDK is .NET 8.0 which will NOT work.
- Install .NET 9.0 SDK first:
  ```bash
  wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
  chmod +x dotnet-install.sh
  ./dotnet-install.sh --version 9.0.101
  export PATH="/home/runner/.dotnet:$PATH"
  ```
- Always set the PATH in every new session: `export PATH="/home/runner/.dotnet:$PATH"`
- Verify installation: `dotnet --version` should return `9.0.101` or higher

### Building and Running
- **Restore packages**: `dotnet restore` -- takes 3-7 seconds
- **Build (Debug)**: `dotnet build` -- takes 3-5 seconds. NEVER CANCEL. Set timeout to 120+ seconds.
- **Build (Release)**: `dotnet build --configuration Release` -- takes 1-2 seconds. NEVER CANCEL. Set timeout to 120+ seconds.
- **Run (Development)**: `dotnet run --project DemoWorker` -- starts immediately, logs "Worker running at: [timestamp]"
- **Run (Release)**: `dotnet run --project DemoWorker --configuration Release`
- **Publish**: `dotnet publish DemoWorker --configuration Release` -- takes 1-2 seconds
- **Run Published**: `cd DemoWorker/bin/Release/net9.0/publish && dotnet DemoWorker.dll`

### Code Quality and Formatting
- **Format code**: `dotnet format` -- takes 5-7 seconds. NEVER CANCEL. Set timeout to 180+ seconds.
- **Verify formatting**: `dotnet format --verify-no-changes` -- returns exit code 2 if formatting is needed
- ALWAYS run `dotnet format` before committing changes to ensure consistent code style
- The project has whitespace formatting rules that are enforced

### Testing
- **No test projects exist** in this repository
- To validate changes: Build successfully and run the application to ensure it starts without errors
- Basic validation: Application should log "Worker running at: [timestamp]" and "Application started. Press Ctrl+C to shut down."

## Validation

### Manual Validation Scenarios
After making changes, ALWAYS validate by running through these scenarios:

1. **Basic Build and Run Test**:
   ```bash
   export PATH="/home/runner/.dotnet:$PATH"
   dotnet build
   dotnet run --project DemoWorker
   ```
   - Should build without errors
   - Should start and log worker activity
   - Press Ctrl+C to stop

2. **Code Quality Validation**:
   ```bash
   dotnet format --verify-no-changes
   ```
   - Should return exit code 0 (or run `dotnet format` first if needed)

3. **Release Build Validation**:
   ```bash
   dotnet build --configuration Release
   dotnet publish DemoWorker --configuration Release
   ```
   - Both should complete without errors

### Functional Testing
- The application is a background worker service that runs continuously
- Main functionality is in the Worker class which logs timestamp information
- HTTP client services are available but not actively used in the base worker loop
- Application uses dependency injection with services registered in ServiceCollectionExtensions

## Project Structure

### Key Directories and Files
```
DemoWorker/                           # Main project directory
├── Program.cs                        # Application entry point and service registration
├── Worker.cs                         # Main background worker service
├── ServiceCollectionExtensions.cs   # Dependency injection configuration
├── appsettings.json                 # Configuration settings
├── Properties/launchSettings.json   # Development launch configuration
├── Services/                        # HTTP client services
│   └── HttpClientService.cs        # Generic HTTP client wrapper
├── Interfaces/                      # Service contracts
│   ├── IHttpClientService.cs       # HTTP client interface
│   ├── IAuthorizationHandler.cs    # Auth handler interface
│   └── IResponseProcessor.cs       # Response processor interface
├── Handlers/                       # Request/response handlers
│   └── AuthorizationHandler.cs     # HTTP authorization implementation
├── Processors/                     # Data processors
│   └── ResponseProcessor.cs        # HTTP response processing logic
├── Models/                         # Data models
│   ├── BaseResponse.cs            # Base response model
│   └── Request.cs                 # Generic request model
├── Enums/                          # Enumerations
│   └── AuthenticationType.cs      # Authentication type enum
└── Constants/                      # Application constants
    └── ContentTypeConstant.cs      # HTTP content type constants
```

### Common Editing Patterns
- When modifying HTTP services, check both the interface and implementation
- Service registration happens in ServiceCollectionExtensions.cs
- Configuration changes go in appsettings.json
- Worker logic modifications are in Worker.cs
- New services need to be registered in ServiceCollectionExtensions.cs

## Common Commands Reference

### Project Root Contents
```
$ ls -la
.git/
.gitignore
DemoWorker/
WorkerService.sln
dotnet-install.sh
```

### DemoWorker Project Contents
```
$ ls -la DemoWorker/
Constants/
DemoWorker.csproj
Enums/
Handlers/
Interfaces/
Models/
Processors/
Program.cs
Properties/
ServiceCollectionExtensions.cs
Services/
Worker.cs
appsettings.json
```

### Key Project Configuration
```xml
<!-- DemoWorker.csproj -->
<Project Sdk="Microsoft.NET.Sdk.Worker">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.6" />
    <PackageReference Include="Microsoft.Extensions.Http" Version="9.0.8" />
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
    <PackageReference Include="System.Net.Http.Json" Version="9.0.8" />
  </ItemGroup>
</Project>
```

## Troubleshooting

### Common Issues
1. **"NETSDK1045: The current .NET SDK does not support targeting .NET 9.0"**
   - Solution: Install .NET 9.0 SDK using the commands in Prerequisites section

2. **Build fails with package restore errors**
   - Solution: Run `dotnet restore` first, ensure internet connectivity

3. **Application doesn't start**
   - Check that build succeeded first
   - Verify .NET 9.0 SDK is in PATH
   - Check appsettings.json for configuration issues

4. **Formatting errors during CI**
   - Solution: Run `dotnet format` before committing changes

### Performance Notes
- Initial builds take 3-7 seconds (includes package restore)
- Subsequent builds take 1-5 seconds
- Application startup is immediate (under 1 second)
- Code formatting takes 5-7 seconds
- .NET 9.0 installation takes ~30 seconds (one-time setup)

## Development Workflow
1. Always ensure .NET 9.0 SDK is available and in PATH
2. Run `dotnet restore` for new checkouts (optional, included in build)
3. Use `dotnet build` for development builds
4. Run `dotnet format` before committing to ensure code consistency
5. Test with `dotnet run --project DemoWorker` to verify functionality
6. Use `dotnet build --configuration Release` for production builds
7. **CRITICAL**: Always validate changes by running the application and verifying it logs "Worker running at: [timestamp]"