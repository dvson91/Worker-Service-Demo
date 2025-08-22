using DemoWorker.Models;

namespace DemoWorker.Interfaces;

public interface ITokenService
{
    Task<TokenResponse?> GetMockTokenAsync();
    Task<GraphGroupMemberResponse?> GetMockGroupMembersAsync();
    
    /// <summary>
    /// Gets an SSO token for the current context. Implementation may vary based on the environment.
    /// </summary>
    Task<string> GetSsoTokenAsync();
}
