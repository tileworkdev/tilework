using Tilework.Ui.Mappers;

namespace Tilework.Ui.ViewModels;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserInterface(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(FormMappingProfile));

        // Load balancer
        services.AddScoped<LoadBalancerEditViewModel>();

        services.AddScoped<TargetGroupEditViewModel>();

        return services;
    }
}
