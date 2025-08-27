using DemoWorker.Entities;
using DemoWorker.Interfaces;
using DemoWorker.Models;
using Microsoft.Extensions.Configuration;

namespace DemoWorker.Services;

public class UserRoleSyncService : IUserRoleSyncService
{
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IHttpClientService _httpClientService;
    private readonly ITokenManager _tokenManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UserRoleSyncService> _logger;

    public UserRoleSyncService(
        IUserRoleRepository userRoleRepository,
        IHttpClientService httpClientService,
        ITokenManager tokenManager,
        IConfiguration configuration,
        ILogger<UserRoleSyncService> logger)
    {
        _userRoleRepository = userRoleRepository;
        _httpClientService = httpClientService;
        _tokenManager = tokenManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SyncUserRolesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting UserRole synchronization from OneIDM");

            // Get data from OneIDM API
            var apiData = await FetchUserRolesFromApiAsync(cancellationToken);
            if (apiData == null || !apiData.Any())
            {
                _logger.LogWarning("No data received from OneIDM API");
                return false;
            }

            // Convert API data to UserRole entities
            var userRoles = ConvertApiDataToUserRoles(apiData);

            // Clear existing data and insert new data (full sync)
            await _userRoleRepository.DeleteAllAsync();
            var insertedCount = await _userRoleRepository.BulkInsertAsync(userRoles);

            _logger.LogInformation("Successfully synchronized {Count} user roles from OneIDM", insertedCount);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during UserRole synchronization");
            return false;
        }
    }

    private async Task<List<OneIdmModule>?> FetchUserRolesFromApiAsync(CancellationToken cancellationToken)
    {
        try
        {
            var apiUrl = _configuration["OneIdm:ApiUrl"];

            if (string.IsNullOrEmpty(apiUrl))
            {
                _logger.LogError("OneIdm API URL not configured");
                return null;
            }

            // Get valid Bearer token from token manager
            var bearerToken = await _tokenManager.GetValidTokenAsync(cancellationToken);

            var request = new Request<object>
            {
                Url = apiUrl,
                Token = bearerToken,
                AuthenticationType = AuthenticationType.Bearer
            };

            var response = await _httpClientService.SendGetAsync<object, OneIdmApiResponse>(request);

            if (!response.IsSuccess)
            {
                _logger.LogError("Failed to fetch data from OneIDM API: {Message}", response.Message);

                // If unauthorized, invalidate token and retry once
                if (response.Message?.Contains("401") == true || response.Message?.Contains("Unauthorized") == true)
                {
                    _logger.LogWarning("Received unauthorized response, invalidating token and retrying");
                    _tokenManager.InvalidateToken();

                    var newBearerToken = await _tokenManager.GetValidTokenAsync(cancellationToken);
                    request.Token = newBearerToken;

                    response = await _httpClientService.SendGetAsync<object, OneIdmApiResponse>(request);

                    if (!response.IsSuccess)
                    {
                        _logger.LogError("Retry after token refresh also failed: {Message}", response.Message);
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }

            return response.Modules;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching data from OneIDM API");
            return null;
        }
    }

    private List<UserRole> ConvertApiDataToUserRoles(List<OneIdmModule> modules)
    {
        var userRoles = new List<UserRole>();

        foreach (var module in modules)
        {
            foreach (var role in module.Roles)
            {
                foreach (var user in role.Users)
                {
                    userRoles.Add(new UserRole
                    {
                        DomainId = user.DomainId,
                        Role = role.RoleName,
                        Module = module.ModuleName
                    });
                }
            }
        }

        _logger.LogDebug("Converted {ModuleCount} modules into {UserRoleCount} user roles",
            modules.Count, userRoles.Count);

        return userRoles;
    }
}