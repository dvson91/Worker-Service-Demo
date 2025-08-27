namespace DemoWorker;

public class HttpClientService : IHttpClientService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IResponseProcessor _responseProcessor;
    private readonly IAuthorizationHandler _authHandler;
    private readonly ILogger<HttpClientService> _logger;
    private readonly string _clientName;

    public HttpClientService(
        IHttpClientFactory httpClientFactory,
        IResponseProcessor responseProcessor,
        IAuthorizationHandler authHandler,
        ILogger<HttpClientService> logger,
        string clientName = "oneIDM")
    {
        _httpClientFactory = httpClientFactory;
        _responseProcessor = responseProcessor;
        _authHandler = authHandler;
        _logger = logger;
        _clientName = clientName;
    }

    public async Task<TResponse> SendGetAsync<TRequest, TResponse>(Request<TRequest> request)
            where TResponse : BaseResponse, new()
    {
        try
        {
            _logger.LogInformation("Sending GET request to {Url} using client {ClientName}", request.Url, _clientName);

            using var client = _authHandler.CreateClientWithAuth(_httpClientFactory, _clientName, request);
            using var response = await client.GetAsync(request.Url);

            return await _responseProcessor.ProcessResponse<TResponse>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error for GET request to {Url}", request?.Url);
            return new TResponse
            {
                IsSuccess = false,
                Message = $"Unexpected error: {ex.Message}",
                StatusCode = 500
            };
        }
    }
}
