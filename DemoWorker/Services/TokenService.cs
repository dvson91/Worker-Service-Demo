using DemoWorker.Interfaces;
using DemoWorker.Models;
using DemoWorker.Options;
using Microsoft.Extensions.Options;

namespace DemoWorker.Services;

public class TokenService : ITokenService
{
    private readonly ILogger<TokenService> _logger;
    private readonly GraphApiOptions _options;
    
    public TokenService(ILogger<TokenService> logger, IOptions<GraphApiOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }
    
    /// <summary>
    /// In a real-world application, this would be a method to get the user's SSO token
    /// from a session, database, or other source. For demo purposes, we're just returning a mock token.
    /// </summary>
    /// <returns>A mocked token response for demonstration purposes</returns>
    public Task<Models.TokenResponse?> GetMockTokenAsync()
    {
        _logger.LogInformation("Generating mock token for demonstration purposes");
        
        // For demonstration only - in a real application, you would get a real token
        return Task.FromResult<Models.TokenResponse?>(new Models.TokenResponse
        {
            Access_token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6IjVCM25SeHRRN2ppOGVORGMzRnkwNUtmOTdaRSJ9.eyJhdWQiOiJodHRwczovL2dyYXBoLm1pY3Jvc29mdC5jb20iLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC8wYWU1MWUxOS0wN2M4LTRlNGItYmI2ZC02NDhlZTU4NDEwZjQvIiwiaWF0IjoxNjkzNTI5MTQ4LCJuYmYiOjE2OTM1MjkxNDgsImV4cCI6MTY5MzUzMzA0OCwiYWlvIjoiRTJGZ1lGQi9tTlNUN2h1cDAyRkc3M3hHREFBPSIsImFwcF9kaXNwbGF5bmFtZSI6IkRlbW9Xb3JrZXIiLCJhcHBpZCI6IjcwODRlMjliLTdkYzQtNGNhNC1hOGJiLWZmZmM3ZDJmMTU4MSIsImFwcGlkYWNyIjoiMSIsImlkcCI6Imh0dHBzOi8vc3RzLndpbmRvd3MubmV0LzBhZTUxZTE5LTA3YzgtNGU0Yi1iYjZkLTY0OGVlNTg0MTBmNC8iLCJpZHR5cCI6ImFwcCIsIm9pZCI6IjI5YzExNTU3LTdiNWItNDU3MC05MTkzLTNhOTUzZmVhMmE2YSIsInJoIjoiMC5BUkVBeTJ6a0UtLXRaVW1PbFBfczk1V2ZqNGRaNkZGSXRQTkxqWVp1LVZ4R2RBQUEuIiwicm9sZXMiOlsiVXNlci5SZWFkLkFsbCIsIkdyb3VwLlJlYWQuQWxsIl0sInN1YiI6IjI5YzExNTU3LTdiNWItNDU3MC05MTkzLTNhOTUzZmVhMmE2YSIsInRlbmFudF9yZWdpb25fc2NvcGUiOiJBUyIsInRpZCI6IjBhZTUxZTE5LTA3YzgtNGU0Yi1iYjZkLTY0OGVlNTg0MTBmNCIsInV0aSI6ImpDWVFLcmRlLTBpbDBzOERfUE1MQUEiLCJ2ZXIiOiIxLjAiLCJ3aWRzIjpbIjA5OTdhMWQwLTBkMWQtNGFjYi1iNDA4LWQ1Y2E3MzEyMWU5MCJdLCJ4bXNfdGNkdCI6MTU5NDgwNzQ4OX0.SignatureWouldBeHere",
            Token_type = "Bearer",
            Expires_in = 3599,
            Ext_expires_in = 3599,
            Scope = "https://graph.microsoft.com/.default"
        });
    }

    /// <summary>
    /// Gets an SSO token for the current context. In a real scenario, this would interact with
    /// a token provider or other authentication mechanism.
    /// </summary>
    public Task<string> GetSsoTokenAsync()
    {
        _logger.LogInformation("Getting SSO token");
        
        // In a real-world scenario, this would get an actual token from a token provider
        // For now, we just return a placeholder string
        return Task.FromResult("sso_token_user");
    }
    
    /// <summary>
    /// In a real-world application, this would get a real group members response.
    /// For demo purposes, we're just returning mock data.
    /// </summary>
    public Task<Models.GraphGroupMemberResponse?> GetMockGroupMembersAsync()
    {
        _logger.LogInformation("Generating mock group members for demonstration purposes");
        
        // For demonstration only - in a real application, this would come from the Graph API
        return Task.FromResult<Models.GraphGroupMemberResponse?>(new Models.GraphGroupMemberResponse
        {
            Value = new List<Models.GraphUser>
            {
                new() { Id = "user1", DisplayName = "John Doe", UserPrincipalName = "john.doe@example.com" },
                new() { Id = "user2", DisplayName = "Jane Smith", UserPrincipalName = "jane.smith@example.com" },
                new() { Id = "user3", DisplayName = "Bob Johnson", UserPrincipalName = "bob.johnson@example.com" }
            }
        });
    }
}
