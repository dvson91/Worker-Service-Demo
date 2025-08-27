using System.Net.Http.Headers;

namespace DemoWorker;

public class AuthorizationHandler : IAuthorizationHandler
{
    public HttpClient CreateClientWithAuth<TRequest>(IHttpClientFactory factory, string clientName, Request<TRequest> request)
    {
        var client = factory.CreateClient(clientName);

        if (!string.IsNullOrEmpty(request.Token))
        {
            var authScheme = request.AuthenticationType.ToString();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authScheme, request.Token);
        }

        return client;
    }
}
