namespace Tilework.Ui.ViewModels;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserInterface(this IServiceCollection services)
    {
        // Load balancer
        services.AddScoped<LoadBalancerListViewModel>();
        services.AddScoped<LoadBalancerDetailViewModel>();
        services.AddScoped<LoadBalancerNewViewModel>();
        services.AddScoped<LoadBalancerEditViewModel>();

        services.AddScoped<TargetGroupListViewModel>();
        services.AddScoped<TargetGroupDetailViewModel>();
        services.AddScoped<TargetGroupNewViewModel>();
        services.AddScoped<TargetGroupEditViewModel>();

        // Certificate management
        services.AddScoped<CertificateAuthorityListViewModel>();
        services.AddScoped<CertificateAuthorityNewViewModel>();
        services.AddScoped<CertificateListViewModel>();
        services.AddScoped<CertificateNewViewModel>();


        return services;
    }
}
