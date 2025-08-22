using System.Net.Http.Headers;
using System.Net.Http.Json;
using DemoWorker.Interfaces;
using DemoWorker.Models;
using DemoWorker.Options;
using Microsoft.Extensions.Options;

namespace DemoWorker.Services;

public class GraphApiService : IGraphApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GraphApiService> _logger;
    private readonly GraphApiOptions _options;

    // MS Entra ID settings
    private readonly string _tenantId;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _groupId;

    public GraphApiService(
        HttpClient httpClient, 
        ILogger<GraphApiService> logger, 
        IOptions<GraphApiOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
        
        // Get settings from options
        _tenantId = _options.TenantId;
        _clientId = _options.ClientId;
        _clientSecret = _options.ClientSecret;
        _groupId = _options.GroupId;
    }

    public async Task<TokenResponse?> GetDelegatedTokenAsync(string ssoToken)
    {
        try
        {
            // OAuth 2.0 token endpoints generally require POST method
            // Note: While the curl command may use --request GET, when combined with --data-urlencode,
            // curl automatically switches to POST method regardless of the specified request type
            var tokenEndpoint = $"https://login.microsoftonline.com/{_tenantId}/oauth2/v2.0/token";
            
            // Prepare request content
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = _clientId,
                ["scope"] = "https://graph.microsoft.com/.default",
                ["client_secret"] = _clientSecret,
                ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                ["assertion"] = ssoToken,
                ["requested_token_use"] = "on_behalf_of"
            });

            // Send request using POST method as per OAuth 2.0 specification
            var response = await _httpClient.PostAsync(tokenEndpoint, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to get delegated token. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);
                return null;
            }

            // Deserialize the response
            return await response.Content.ReadFromJsonAsync<TokenResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while getting delegated token");
            return null;
        }
    }

    public async Task<GraphGroupMemberResponse?> GetGroupMembersAsync(string accessToken)
    {
        try
        {
            var endpoint = $"https://graph.microsoft.com/v1.0/groups/{_groupId}/members";
            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            
            var response = await _httpClient.GetAsync(endpoint);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to get group members. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);
                return null;
            }

            // Deserialize the response
            return await response.Content.ReadFromJsonAsync<GraphGroupMemberResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while getting group members");
            return null;
        }
    }
}
