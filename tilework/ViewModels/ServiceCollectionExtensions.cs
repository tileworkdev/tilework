using Microsoft.Extensions.DependencyInjection;

namespace Tilework.LoadBalancing.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddViewmodels(this IServiceCollection services)
    {
        services.AddScoped<LoadBalancerService>();

        return services;
    }
}
