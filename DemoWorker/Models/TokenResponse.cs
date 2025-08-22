namespace DemoWorker.Models;

public class TokenResponse
{
    public string? Token_type { get; set; }
    public string? Scope { get; set; }
    public int Expires_in { get; set; }
    public int Ext_expires_in { get; set; }
    public string? Access_token { get; set; }
}
