using DemoWorker.Models;
using Refit;

namespace DemoWorker.Clients;

public interface IOneIdmTokenClient
{
    [Post("/token")]
    Task<TokenResponse> GetTokenAsync([Body] TokenRequest request, CancellationToken cancellationToken = default);
}