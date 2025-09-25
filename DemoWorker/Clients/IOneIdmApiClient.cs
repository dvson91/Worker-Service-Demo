using DemoWorker.Models;
using Refit;

namespace DemoWorker.Clients;

public interface IOneIdmApiClient
{
    [Get("/api/userroles")]
    [Headers("Content-Type: application/json")]
    Task<OneIdmApiResponse> GetUserRolesAsync([Header("Authorization")] string authorization, CancellationToken cancellationToken = default);
}