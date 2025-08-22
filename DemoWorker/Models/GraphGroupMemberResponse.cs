namespace DemoWorker.Models;

public class GraphGroupMemberResponse
{
    public List<GraphUser>? Value { get; set; }
}

public class GraphUser
{
    public string? Id { get; set; }
    public string? DisplayName { get; set; }
    public string? UserPrincipalName { get; set; }
    // Add other properties as needed based on the actual response
}
