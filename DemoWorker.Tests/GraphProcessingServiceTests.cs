using DemoWorker.Interfaces;
using DemoWorker.Models;
using DemoWorker.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;

namespace DemoWorker.Tests;

public class GraphProcessingServiceTests
{
    private readonly Mock<ILogger<GraphProcessingService>> _loggerMock;
    private readonly Mock<IGraphApiService> _graphApiServiceMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IHostEnvironment> _envMock;
    
    public GraphProcessingServiceTests()
    {
        _loggerMock = new Mock<ILogger<GraphProcessingService>>();
        _graphApiServiceMock = new Mock<IGraphApiService>();
        _tokenServiceMock = new Mock<ITokenService>();
        _envMock = new Mock<IHostEnvironment>();
    }
    
    [Fact]
    public async Task ProcessGraphApiRequests_InDevelopment_UsesMockData()
    {
        // Arrange
        _envMock.Setup(e => e.EnvironmentName).Returns("Development");
        
        var mockToken = new TokenResponse
        {
            Access_token = "mock_token",
            Expires_in = 3600
        };
        
        var mockGroupMembers = new GraphGroupMemberResponse
        {
            Value = new List<GraphUser>
            {
                new() { Id = "user1", DisplayName = "Test User" }
            }
        };
        
        _tokenServiceMock.Setup(ts => ts.GetMockTokenAsync()).ReturnsAsync(mockToken);
        _tokenServiceMock.Setup(ts => ts.GetMockGroupMembersAsync()).ReturnsAsync(mockGroupMembers);
        
        var service = new GraphProcessingService(
            _loggerMock.Object,
            _graphApiServiceMock.Object,
            _tokenServiceMock.Object,
            _envMock.Object);
            
        // Act
        var result = await service.ProcessGraphApiRequestsAsync();
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(mockToken, result.TokenResponse);
        Assert.Single(result.GroupMembers);
        Assert.Equal("Test User", result.GroupMembers[0].DisplayName);
        
        // Verify mock calls
        _tokenServiceMock.Verify(ts => ts.GetMockTokenAsync(), Times.Once);
        _tokenServiceMock.Verify(ts => ts.GetMockGroupMembersAsync(), Times.Once);
        _graphApiServiceMock.Verify(gs => gs.GetDelegatedTokenAsync(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task ProcessGraphApiRequests_InProduction_UsesRealApi()
    {
        // Arrange
        _envMock.Setup(e => e.EnvironmentName).Returns("Production");
        
        var ssoToken = "real_sso_token";
        var delegatedToken = new TokenResponse
        {
            Access_token = "real_delegated_token",
            Expires_in = 3600
        };
        
        var groupMembers = new GraphGroupMemberResponse
        {
            Value = new List<GraphUser>
            {
                new() { Id = "user2", DisplayName = "Real User" }
            }
        };
        
        _tokenServiceMock.Setup(ts => ts.GetSsoTokenAsync()).ReturnsAsync(ssoToken);
        _graphApiServiceMock.Setup(gs => gs.GetDelegatedTokenAsync(ssoToken)).ReturnsAsync(delegatedToken);
        _graphApiServiceMock.Setup(gs => gs.GetGroupMembersAsync(delegatedToken.Access_token)).ReturnsAsync(groupMembers);
        
        var service = new GraphProcessingService(
            _loggerMock.Object,
            _graphApiServiceMock.Object,
            _tokenServiceMock.Object,
            _envMock.Object);
            
        // Act
        var result = await service.ProcessGraphApiRequestsAsync();
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(delegatedToken, result.TokenResponse);
        Assert.Single(result.GroupMembers);
        Assert.Equal("Real User", result.GroupMembers[0].DisplayName);
        
        // Verify mock calls
        _tokenServiceMock.Verify(ts => ts.GetSsoTokenAsync(), Times.Once);
        _graphApiServiceMock.Verify(gs => gs.GetDelegatedTokenAsync(ssoToken), Times.Once);
        _graphApiServiceMock.Verify(gs => gs.GetGroupMembersAsync(delegatedToken.Access_token), Times.Once);
    }
}
