namespace DemoWorker;

public class Request<T>
{
    public string Url { get; set; } = string.Empty;
    public string ContentType { get; set; } = ContentTypeConstant.Json;
    public AuthenticationType AuthenticationType { get; set; } = AuthenticationType.Bearer;
    public string Token { get; set; } = string.Empty;
    public T? Body { get; set; }
}
