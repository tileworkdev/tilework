using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

using Tilework.LoadBalancing.Haproxy;
using Tilework.LoadBalancing.ViewModels;
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

        services.AddScoped<LoadBalancerService>();
        services.AddScoped<HAProxyConfigurator>();

        services.AddDbContext<LoadBalancerContext>(dbContextOptions);

        services.AddScoped<LoadBalancerListViewModel>();
        services.AddScoped<LoadBalancerDetailViewModel>();
        services.AddScoped<LoadBalancerNewViewModel>();
        services.AddScoped<LoadBalancerEditViewModel>();

        services.AddScoped<TargetGroupListViewModel>();
        services.AddScoped<TargetGroupDetailViewModel>();
        services.AddScoped<TargetGroupNewViewModel>();
        services.AddScoped<TargetGroupEditViewModel>();

        return services;
    }
}
