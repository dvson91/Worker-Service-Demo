namespace DemoWorker.Options;

public class GraphApiOptions
{
    public const string SectionName = "GraphApi";
    
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
    public int ProcessingInterval { get; set; } = 60;
}
