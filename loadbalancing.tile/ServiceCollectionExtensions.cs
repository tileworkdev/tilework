using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

using Tilework.LoadBalancing.Haproxy;
using Tilework.LoadBalancing.Settings;
using Tilework.LoadBalancing.Persistence;

namespace Tilework.LoadBalancing.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLoadBalancing(this IServiceCollection services,
                                                      IConfiguration configuration,
                                                      Action<DbContextOptionsBuilder> dbContextOptions)
    {
        services.Configure<LoadBalancerSettings>(configuration);

        services.AddAutoMapper(typeof(HAProxyProfile));

        services.AddScoped<LoadBalancerService>();
        services.AddScoped<HAProxyConfigurator>();

        services.AddDbContext<LoadBalancerContext>(dbContextOptions);

        return services;
    }
}
