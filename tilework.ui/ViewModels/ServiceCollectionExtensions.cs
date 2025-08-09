using Tilework.Ui.Mappers;

namespace Tilework.Ui.ViewModels;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserInterface(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(FormMappingProfile));

        // Load balancer
        services.AddScoped<LoadBalancerDetailViewModel>();
        services.AddScoped<LoadBalancerEditViewModel>();

        services.AddScoped<TargetGroupDetailViewModel>();
        services.AddScoped<TargetGroupEditViewModel>();

        // Certificate management
        services.AddScoped<CertificateAuthorityNewViewModel>();
        services.AddScoped<CertificateNewViewModel>();


        return services;
    }
}
