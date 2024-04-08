using Microsoft.Extensions.DependencyInjection;

namespace Tilework.ViewModels;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {

        services.AddScoped<LoadBalancerListViewModel>();
        services.AddScoped<LoadBalancerDetailViewModel>();

        return services;
    }
}
