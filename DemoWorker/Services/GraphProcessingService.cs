using DemoWorker.Interfaces;
using DemoWorker.Models;

namespace DemoWorker.Services;

public interface IGraphProcessingService
{
    Task<GraphProcessingResult> ProcessGraphApiRequestsAsync();
}

public class GraphProcessingService : IGraphProcessingService
{
    private readonly ILogger<GraphProcessingService> _logger;
    private readonly IGraphApiService _graphApiService;
    private readonly ITokenService _tokenService;
    private readonly IHostEnvironment _environment;

    public GraphProcessingService(
        ILogger<GraphProcessingService> logger,
        IGraphApiService graphApiService,
        ITokenService tokenService,
        IHostEnvironment environment)
    {
        _logger = logger;
        _graphApiService = graphApiService;
        _tokenService = tokenService;
        _environment = environment;
    }

    public async Task<GraphProcessingResult> ProcessGraphApiRequestsAsync()
    {
        _logger.LogInformation("Processing Graph API requests at: {time}", DateTimeOffset.Now);

        try
        {
            TokenResponse? tokenResponse;
            GraphGroupMemberResponse? groupMembersResponse;

            if (_environment.EnvironmentName == "Development")
            {
                // In development mode, use mock data
                _logger.LogInformation("Running in development mode - using mock data");
                tokenResponse = await _tokenService.GetMockTokenAsync();
                groupMembersResponse = await _tokenService.GetMockGroupMembersAsync();
            }
            else
            {
                // In production mode, use the real APIs
                // Step 1: Get SSO token
                string ssoToken = await _tokenService.GetSsoTokenAsync();
                
                // Step 2: Get delegated token using the SSO token
                tokenResponse = await _graphApiService.GetDelegatedTokenAsync(ssoToken);
                
                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Access_token))
                {
                    _logger.LogWarning("Failed to get delegated access token");
                    return GraphProcessingResult.Failed("Failed to get delegated access token");
                }

                // Step 3: Get group members using the acquired token
                groupMembersResponse = await _graphApiService.GetGroupMembersAsync(tokenResponse.Access_token);
                
                if (groupMembersResponse == null || groupMembersResponse.Value == null)
                {
                    _logger.LogWarning("Failed to get group members");
                    return GraphProcessingResult.Failed("Failed to get group members");
                }
            }

            _logger.LogInformation("Successfully obtained delegated access token. Expires in: {ExpiresIn} seconds", 
                tokenResponse?.Expires_in ?? 0);

            // Process the group members
            int memberCount = groupMembersResponse?.Value?.Count ?? 0;
            _logger.LogInformation("Successfully retrieved {Count} group members", memberCount);
            
            if (groupMembersResponse?.Value != null)
            {
                foreach (var user in groupMembersResponse.Value)
                {
                    _logger.LogInformation("User: {DisplayName}, Email: {Email}", 
                        user.DisplayName, 
                        user.UserPrincipalName);
                }
            }
            
            return new GraphProcessingResult
            {
                IsSuccess = true,
                TokenResponse = tokenResponse,
                GroupMembers = groupMembersResponse?.Value ?? new List<GraphUser>(),
                ProcessedAt = DateTimeOffset.Now
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during API processing");
            return GraphProcessingResult.Failed(ex.Message);
        }
    }
}
