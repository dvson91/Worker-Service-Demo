using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DemoWorker.Health;

public class GraphApiHealthCheck : IHealthCheck
{
    private readonly ILogger<GraphApiHealthCheck> _logger;
    private readonly IServiceProvider _serviceProvider;
    
    public GraphApiHealthCheck(ILogger<GraphApiHealthCheck> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }
    
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // In a real-world scenario, you might want to check the connectivity to the Graph API
            // by making a lightweight request
            
            // For now, we'll just return a healthy result
            return Task.FromResult(HealthCheckResult.Healthy("GraphAPI service is healthy"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GraphAPI health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy("GraphAPI service is unhealthy", ex));
        }
    }
}
