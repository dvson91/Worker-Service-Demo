using DemoWorker;
using DemoWorker.Health;
using DemoWorker.Interfaces;
using DemoWorker.Options;
using DemoWorker.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = Host.CreateApplicationBuilder(args);

// Configure options
builder.Services.Configure<GraphApiOptions>(
    builder.Configuration.GetSection(GraphApiOptions.SectionName));

// Register hosted service
builder.Services.AddHostedService<Worker>();

// Register HTTP clients
builder.Services.AddHttpClient<IGraphApiService, GraphApiService>();

// Register services with their interfaces
// Singleton for services used by hosted service
builder.Services.AddSingleton<IGraphApiService, GraphApiService>();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddSingleton<IGraphProcessingService, GraphProcessingService>();

// Register health checks
builder.Services.AddHealthChecks()
    .AddCheck<GraphApiHealthCheck>("graph_api");

// Register other services
builder.Services.AddLogging();

var host = builder.Build();
host.Run();
