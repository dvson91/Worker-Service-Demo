using DemoWorker.Options;
using DemoWorker.Services;
using Microsoft.Extensions.Options;

namespace DemoWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IGraphProcessingService _graphProcessingService;
    private readonly GraphApiOptions _options;

    public Worker(
        ILogger<Worker> logger,
        IGraphProcessingService graphProcessingService,
        IOptions<GraphApiOptions> options)
    {
        _logger = logger;
        _graphProcessingService = graphProcessingService;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker starting at: {time}", DateTimeOffset.Now);
        
        // Process once on startup
        await ProcessAndLogResults();

        // Then continue with periodic processing
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            
            try
            {
                await ProcessAndLogResults();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during API processing");
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.ProcessingInterval), stoppingToken);
        }
    }

    private async Task ProcessAndLogResults()
    {
        var result = await _graphProcessingService.ProcessGraphApiRequestsAsync();
        
        if (result.IsSuccess)
        {
            _logger.LogInformation("API processing completed successfully at {ProcessedAt}. Found {MemberCount} group members.", 
                result.ProcessedAt, 
                result.GroupMembers.Count);
        }
        else
        {
            _logger.LogWarning("API processing failed: {ErrorMessage}", result.ErrorMessage);
        }
    }
}
