using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

using Tilework.Core.Interfaces;
using Tilework.LoadBalancing.Interfaces;
using Tilework.CertificateManagement.Interfaces;

using Tilework.LoadBalancing.Models;

namespace Tilework.Core.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<IServiceManager, SystemdServiceManager>();
        services.AddSingleton<IContainerManager, DockerServiceManager>();
        services.AddHostedService<CoreInitializer>();

        return services;
    }

    public static IServiceCollection AddLoadBalancing(this IServiceCollection services,
                                                      IConfiguration configuration,
                                                      Action<DbContextOptionsBuilder> dbContextOptions)
    {
        services.Configure<LoadBalancerConfiguration>(configuration);

        services.AddAutoMapper(typeof(HAProxyProfile));

        services.AddScoped<ILoadBalancerService, LoadBalancerService>();
        services.AddScoped<HAProxyConfigurator>();

        services.AddDbContext<LoadBalancerContext>(dbContextOptions);

        services.AddHostedService<LoadBalancingInitializer>();

        services.AddAutoMapper(typeof(LoadBalancingMappingProfile));

        return services;
    }


    public static IServiceCollection AddCertificateManagement(this IServiceCollection services,
                                                              IConfiguration configuration,
                                                              Action<DbContextOptionsBuilder> dbContextOptions)
    {
        services.Configure<CertificateManagementSettings>(configuration);

        services.AddScoped<ICertificateManagementService, CertificateManagementService>();

        services.AddScoped<AcmeProvider>();
        services.AddScoped<AcmeVerificationService>();

        services.AddDbContext<CertificateManagementContext>(dbContextOptions);

        services.AddHostedService<CertificateManagementInitializer>();

        services.AddAutoMapper(typeof(CertificateManagementMappingProfile));
        
        return services;
    }
}
