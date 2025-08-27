namespace DemoWorker.Models;

public class OneIdmModule
{
    public string ModuleName { get; set; } = string.Empty;
    public List<OneIdmRole> Roles { get; set; } = new();
}

public class OneIdmRole
{
    public string RoleName { get; set; } = string.Empty;
    public List<OneIdmUser> Users { get; set; } = new();
}

public class OneIdmUser
{
    public string DomainId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
}

public class OneIdmApiResponse : BaseResponse
{
    public List<OneIdmModule> Modules { get; set; } = new();
}