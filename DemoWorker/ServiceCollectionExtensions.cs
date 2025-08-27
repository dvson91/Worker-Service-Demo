using Microsoft.Extensions.DependencyInjection;

namespace DemoWorker;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServiceCollection(this IServiceCollection services)
    {
        // HTTP Client
        services.AddHttpClient();
        
        // Application Services
        services.AddScoped<IAuthorizationHandler, AuthorizationHandler>();
        services.AddScoped<IResponseProcessor, ResponseProcessor>();
        services.AddScoped<IHttpClientService, HttpClientService>();
        
        return services;
    }
}