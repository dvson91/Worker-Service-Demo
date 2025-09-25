using DemoWorker.Models;
using Refit;

namespace DemoWorker.Clients;

/// <summary>
/// Refit client interface for OneIDM token management API
/// </summary>
public interface IOneIdmTokenClient
{
    /// <summary>
    /// Request authentication token from OneIDM
    /// </summary>
    /// <param name="request">Token request containing API key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Token response with access token and expiration</returns>
    [Post("/token")]
    Task<TokenResponse> GetTokenAsync([Body] TokenRequest request, CancellationToken cancellationToken = default);
}