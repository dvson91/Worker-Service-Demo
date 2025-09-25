using DemoWorker.Models;
using Refit;

namespace DemoWorker.Clients;

/// <summary>
/// Refit client interface for OneIDM user roles API
/// </summary>
public interface IOneIdmApiClient
{
    /// <summary>
    /// Retrieve user roles and modules from OneIDM API
    /// </summary>
    /// <param name="authorization">Bearer token for authentication</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response containing modules and user role data</returns>
    [Get("/api/userroles")]
    [Headers("Content-Type: application/json")]
    Task<OneIdmApiResponse> GetUserRolesAsync([Header("Authorization")] string authorization, CancellationToken cancellationToken = default);
}