using DemoWorker.Interfaces;

namespace DemoWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly TimeSpan _syncInterval = TimeSpan.FromHours(6);

    public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("UserRole Sync Worker started at: {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Starting UserRole synchronization cycle at: {time}", DateTimeOffset.Now);
                
                using var scope = _serviceScopeFactory.CreateScope();
                var userRoleSyncService = scope.ServiceProvider.GetRequiredService<IUserRoleSyncService>();
                
                var success = await userRoleSyncService.SyncUserRolesAsync(stoppingToken);
                
                if (success)
                {
                    _logger.LogInformation("UserRole synchronization completed successfully");
                }
                else
                {
                    _logger.LogWarning("UserRole synchronization failed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during UserRole synchronization");
            }

            _logger.LogInformation("Next synchronization scheduled in {Hours} hours", _syncInterval.TotalHours);
            await Task.Delay(_syncInterval, stoppingToken);
        }

        _logger.LogInformation("UserRole Sync Worker stopped at: {time}", DateTimeOffset.Now);
    }
}
