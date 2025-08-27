namespace DemoWorker;

public interface IHttpClientService
{
    Task<TResponse> SendGetAsync<TRequest, TResponse>(Request<TRequest> request) where TResponse : BaseResponse, new();
}