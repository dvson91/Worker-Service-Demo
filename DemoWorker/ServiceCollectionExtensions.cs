using DemoWorker.Data;
using DemoWorker.Interfaces;
using DemoWorker.Models;
using DemoWorker.Repositories;
using DemoWorker.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DemoWorker;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServiceCollection(this IServiceCollection services, IConfiguration configuration)
    {
        // HTTP Client
        services.AddHttpClient();

        // Configuration
        services.Configure<OneIdmConfig>(configuration.GetSection("OneIdm"));

        // Application Services
        services.AddScoped<IAuthorizationHandler, AuthorizationHandler>();
        services.AddScoped<IResponseProcessor, ResponseProcessor>();
        services.AddScoped<IHttpClientService, HttpClientService>();

        // Token Management
        services.AddSingleton<ITokenManager, OneIdmTokenManager>();

        // Data Access Services
        services.AddScoped<IDapperContext, DapperContext>();

        // Repository Services
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();

        // Business Logic Services
        services.AddScoped<IUserRoleSyncService, UserRoleSyncService>();

        return services;
    }
}