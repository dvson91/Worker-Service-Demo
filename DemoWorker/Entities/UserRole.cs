namespace DemoWorker.Entities;

public class UserRole
{
    public int Id { get; set; }
    public string DomainId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}