namespace DemoWorker.Models;

public class GraphProcessingResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public TokenResponse? TokenResponse { get; set; }
    public List<GraphUser> GroupMembers { get; set; } = new();
    public DateTimeOffset ProcessedAt { get; set; }
    
    public static GraphProcessingResult Failed(string errorMessage)
    {
        return new GraphProcessingResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ProcessedAt = DateTimeOffset.Now
        };
    }
}
