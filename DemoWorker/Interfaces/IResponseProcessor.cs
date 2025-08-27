namespace DemoWorker;

public interface IResponseProcessor
{
    Task<TResponse> ProcessResponse<TResponse>(HttpResponseMessage response) where TResponse : BaseResponse, new();
}