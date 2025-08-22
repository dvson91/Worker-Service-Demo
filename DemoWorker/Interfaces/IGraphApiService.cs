using DemoWorker.Models;

namespace DemoWorker.Interfaces;

public interface IGraphApiService
{
    Task<TokenResponse?> GetDelegatedTokenAsync(string ssoToken);
    Task<GraphGroupMemberResponse?> GetGroupMembersAsync(string accessToken);
}
