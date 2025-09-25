using DemoWorker.Clients;
using DemoWorker.Data;
using DemoWorker.Interfaces;
using DemoWorker.Models;
using DemoWorker.Repositories;
using DemoWorker.Services;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace DemoWorker;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServiceCollection(this IServiceCollection services, IConfiguration configuration)
    {
        // HTTP Client
        services.AddHttpClient();

        // Configuration
        services.Configure<OneIdmConfig>(configuration.GetSection("OneIdm"));

        // Refit API Clients with HttpClientFactory
        var oneIdmConfig = configuration.GetSection("OneIdm").Get<OneIdmConfig>();

        if (!string.IsNullOrEmpty(oneIdmConfig?.TokenEndpoint))
        {
            var tokenBaseUri = new Uri(oneIdmConfig.TokenEndpoint).GetLeftPart(UriPartial.Authority);
            services.AddRefitClient<IOneIdmTokenClient>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(tokenBaseUri));
        }

        if (!string.IsNullOrEmpty(oneIdmConfig?.ApiUrl))
        {
            var apiBaseUri = new Uri(oneIdmConfig.ApiUrl).GetLeftPart(UriPartial.Authority);
            services.AddRefitClient<IOneIdmApiClient>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseUri));
        }

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