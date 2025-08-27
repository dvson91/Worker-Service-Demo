namespace DemoWorker;

public abstract class BaseResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }
}
