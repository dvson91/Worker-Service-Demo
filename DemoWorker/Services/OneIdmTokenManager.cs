using DemoWorker.Clients;
using DemoWorker.Interfaces;
using DemoWorker.Models;
using Microsoft.Extensions.Options;

namespace DemoWorker.Services;

public class OneIdmTokenManager : ITokenManager
{
    private readonly IOneIdmTokenClient _tokenClient;
    private readonly OneIdmConfig _config;
    private readonly ILogger<OneIdmTokenManager> _logger;
    private readonly SemaphoreSlim _tokenSemaphore = new(1, 1);

    private TokenResponse? _currentToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public OneIdmTokenManager(
        IOneIdmTokenClient tokenClient,
        IOptions<OneIdmConfig> config,
        ILogger<OneIdmTokenManager> logger)
    {
        _tokenClient = tokenClient;
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

            var tokenRequest = new TokenRequest { ApiKey = _config.ApiKey };
            var tokenResponse = await _tokenClient.GetTokenAsync(tokenRequest, cancellationToken);

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