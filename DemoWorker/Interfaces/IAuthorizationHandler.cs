namespace DemoWorker;

public interface IAuthorizationHandler
{
    HttpClient CreateClientWithAuth<TRequest>(IHttpClientFactory factory, string clientName, Request<TRequest> request);
}