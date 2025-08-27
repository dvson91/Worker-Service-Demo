using DemoWorker;

var builder = Host.CreateApplicationBuilder(args);

// Register hosted service
builder.Services.AddHostedService<Worker>();

// Register application services
builder.Services.AddServiceCollection(builder.Configuration);

// Configure logging
builder.Services.AddLogging(configure => configure.AddConsole());

var host = builder.Build();
host.Run();
