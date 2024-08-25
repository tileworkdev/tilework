using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

using Tilework.LoadBalancing.Haproxy;
using Tilework.LoadBalancing.Interfaces;
using Tilework.LoadBalancing.Settings;


namespace Tilework.LoadBalancing.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLoadBalancer(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LoadBalancerSettings>(configuration);

        services.AddScoped<LoadBalancerService>();
        services.AddScoped<ILoadBalancingConfigurator, HAProxyConfigurator>();

        return services;
    }
}
