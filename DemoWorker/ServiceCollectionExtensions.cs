using DemoWorker.Interfaces;
using DemoWorker.Repositories;
using DemoWorker.Services;
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
      
        // Repository Services
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        
        // Business Logic Services
        services.AddScoped<IUserRoleSyncService, UserRoleSyncService>();
        
        return services;
    }
}