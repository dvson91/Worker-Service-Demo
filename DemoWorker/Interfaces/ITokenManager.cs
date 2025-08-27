namespace DemoWorker.Interfaces;

public interface ITokenManager
{
    Task<string> GetValidTokenAsync(CancellationToken cancellationToken = default);
    Task<string> RefreshTokenAsync(CancellationToken cancellationToken = default);
    void InvalidateToken();
}