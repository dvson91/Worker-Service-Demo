using DemoWorker.Interfaces;
using DemoWorker.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text;

namespace DemoWorker.Services;

public class OneIdmTokenManager : ITokenManager
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OneIdmConfig _config;
    private readonly ILogger<OneIdmTokenManager> _logger;
    private readonly SemaphoreSlim _tokenSemaphore = new(1, 1);

    private TokenResponse? _currentToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public OneIdmTokenManager(
        IHttpClientFactory httpClientFactory,
        IOptions<OneIdmConfig> config,
        ILogger<OneIdmTokenManager> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<string> GetValidTokenAsync(CancellationToken cancellationToken = default)
    {
        await _tokenSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (IsTokenValid())
            {
                return _currentToken!.AccessToken;
            }

            return await RefreshTokenAsync(cancellationToken);
        }
        finally
        {
            _tokenSemaphore.Release();
        }
    }

    public async Task<string> RefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Requesting new token from OneIDM");

            if (string.IsNullOrEmpty(_config.ApiKey))
            {
                throw new InvalidOperationException("OneIDM API Key not configured");
            }

            if (string.IsNullOrEmpty(_config.TokenEndpoint))
            {
                throw new InvalidOperationException("OneIDM Token Endpoint not configured");
            }

            var client = _httpClientFactory.CreateClient();

            var tokenRequest = new TokenRequest { ApiKey = _config.ApiKey };
            var jsonContent = JsonSerializer.Serialize(tokenRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(_config.TokenEndpoint, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to obtain token from OneIDM. Status: {StatusCode}, Reason: {ReasonPhrase}",
                    response.StatusCode, response.ReasonPhrase);
                throw new HttpRequestException($"Token request failed: {response.StatusCode} - {response.ReasonPhrase}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                throw new InvalidOperationException("Invalid token response from OneIDM");
            }

            // Set expiry time based on ExpiresIn or configured default
            var expiresIn = tokenResponse.ExpiresIn > 0 ? tokenResponse.ExpiresIn : _config.TokenExpirationMinutes * 60;
            tokenResponse.ExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn);

            _currentToken = tokenResponse;
            _tokenExpiry = tokenResponse.ExpiresAt;

            _logger.LogInformation("Successfully obtained new token from OneIDM. Expires at: {ExpiryTime}", _tokenExpiry);

            return tokenResponse.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token from OneIDM");
            throw;
        }
    }

    public void InvalidateToken()
    {
        _currentToken = null;
        _tokenExpiry = DateTime.MinValue;
        _logger.LogDebug("Token invalidated");
    }

    private bool IsTokenValid()
    {
        if (_currentToken == null || string.IsNullOrEmpty(_currentToken.AccessToken))
        {
            return false;
        }

        // Check if token is expired or will expire within the refresh threshold
        var refreshThreshold = DateTime.UtcNow.AddMinutes(_config.RefreshTokenBeforeExpiryMinutes);
        return _tokenExpiry > refreshThreshold;
    }
}