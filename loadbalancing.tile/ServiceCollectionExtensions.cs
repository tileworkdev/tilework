using Microsoft.Extensions.DependencyInjection;
using Tilework.LoadBalancing.Haproxy;
using Tilework.LoadBalancing.Interfaces;

namespace Tilework.LoadBalancing.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLoadBalancer(this IServiceCollection services)
    {
        services.AddScoped<LoadBalancerService>();
        services.AddScoped<ILoadBalancingConfigurator, HAProxyConfigurator>();

        return services;
    }
}
