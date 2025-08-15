using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

using Tilework.LoadBalancing.Haproxy;
using Tilework.LoadBalancing.Settings;
using Tilework.LoadBalancing.Persistence;
using Tilework.LoadBalancing.Mappers;
using Tilework.Core.Interfaces;

namespace Tilework.LoadBalancing.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLoadBalancing(this IServiceCollection services,
                                                      IConfiguration configuration,
                                                      Action<DbContextOptionsBuilder> dbContextOptions)
    {
        services.Configure<LoadBalancerSettings>(configuration);

        services.AddAutoMapper(typeof(HAProxyProfile));

        services.AddScoped<ILoadBalancerService, LoadBalancerService>();
        services.AddScoped<HAProxyConfigurator>();

        services.AddDbContext<LoadBalancerContext>(dbContextOptions);

        services.AddHostedService<LoadBalancingInitializer>();

        services.AddAutoMapper(typeof(LoadBalancingMappingProfile));

        return services;
    }
}
