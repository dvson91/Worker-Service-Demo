using Newtonsoft.Json;

namespace DemoWorker;

public class ResponseProcessor : IResponseProcessor
{
    private readonly ILogger<ResponseProcessor> _logger;

    public ResponseProcessor(ILogger<ResponseProcessor> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> ProcessResponse<TResponse>(HttpResponseMessage response) where TResponse : BaseResponse, new()
    {
        try
        {
            _logger.LogDebug("Processing HTTP response with status code: {StatusCode}", response.StatusCode);

            var result = new TResponse
            {
                StatusCode = (int)response.StatusCode,
                IsSuccess = response.IsSuccessStatusCode
            };

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(responseContent))
                {
                    try
                    {
                        result = JsonConvert.DeserializeObject<TResponse>(responseContent) ?? result;
                        result.IsSuccess = true;
                        result.StatusCode = (int)response.StatusCode;
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "JSON parsing error for response type {ResponseType}", typeof(TResponse).Name);
                        result.IsSuccess = false;
                        result.Message = $"JSON parsing error: {ex.Message}";
                    }
                }
                else
                {
                    _logger.LogWarning("Received empty response content for successful HTTP request");
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while processing HTTP response");

            return new TResponse
            {
                IsSuccess = false,
                Message = $"Processing error: {ex.Message}",
                StatusCode = 500
            };
        }
    }
}