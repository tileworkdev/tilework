using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

using Coravel;

using Tilework.Core.Interfaces;
using Tilework.LoadBalancing.Interfaces;
using Tilework.CertificateManagement.Interfaces;

using Tilework.LoadBalancing.Models;
using Tilework.LoadBalancing.Services;
using Tilework.LoadBalancing.Mappers;
using Tilework.LoadBalancing.Haproxy;

using Tilework.CertificateManagement.Services;
using Tilework.CertificateManagement.Mappers;
using Tilework.CertificateManagement.Models;

using Tilework.Core.Persistence;

using Tilework.Monitoring.Interfaces;
using Tilework.Monitoring.Telegraf;
using Tilework.Monitoring.Models;
using Tilework.Monitoring.Services;
using Tilework.Monitoring.Influxdb;

using Tilework.TokenVault.Services;

using Tilework.Core.Jobs.CertificateManagement;
using Tilework.Events;

namespace Tilework.Core.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<IServiceManager, SystemdServiceManager>();
        services.AddSingleton<IContainerManager, DockerServiceManager>();
        services.AddSingleton<HttpApiFactoryService>();
        services.AddHostedService<CoreInitializer>();

        services.AddScoped<TokenService>();


        services.AddScheduler();
        services.AddQueue();
        services.AddEvents();

        return services;
    }

    public static IServiceCollection AddMonitoring(this IServiceCollection services,
                                                   IConfiguration configuration,
                                                   Action<DbContextOptionsBuilder> dbContextOptions)
    {
        services.Configure<DataCollectorConfiguration>(configuration.GetSection("DataCollector"));
        services.Configure<DataPersistenceConfiguration>(configuration.GetSection("DataPersistence"));

        services.AddScoped<IDataCollectorConfigurator, TelegrafConfigurator>();
        services.AddScoped<IDataPersistenceConfigurator, Influxdb2Configurator>();
        services.AddScoped<DataCollectorService>();
        services.AddScoped<MonitoringService>();

        services.AddHostedService<MonitoringInitializer>();

        return services;
    }

    public static IServiceCollection AddLoadBalancing(this IServiceCollection services,
                                                      IConfiguration configuration,
                                                      Action<DbContextOptionsBuilder> dbContextOptions)
    {
        services.Configure<LoadBalancerConfiguration>(configuration);

        services.AddAutoMapper(typeof(HAProxyConfigurationProfile));
        services.AddAutoMapper(typeof(HAProxyMonitoringProfile));

        services.AddScoped<ILoadBalancerService, LoadBalancerService>();
        services.AddScoped<HAProxyConfigurator>();
        services.AddScoped<HAProxyMonitor>();

        services.AddDbContext<TileworkContext>(dbContextOptions);

        services.AddHostedService<LoadBalancingInitializer>();

        services.AddAutoMapper(typeof(LoadBalancingMappingProfile));

        services.AddTransient<LoadBalancerCertificateListener>();

        return services;
    }


    public static IServiceCollection AddCertificateManagement(this IServiceCollection services,
                                                              IConfiguration configuration,
                                                              Action<DbContextOptionsBuilder> dbContextOptions)
    {
        services.Configure<CertificateManagementConfiguration>(configuration);

        services.AddScoped<ICertificateManagementService, CertificateManagementService>();

        services.AddScoped<AcmeProvider>();
        services.AddScoped<AcmeVerificationService>();

        services.AddDbContext<TileworkContext>(dbContextOptions);

        services.AddHostedService<CertificateManagementInitializer>();

        services.AddAutoMapper(typeof(CertificateManagementMappingProfile));

        services.AddTransient<CertificateRenewalJob>();
        
        return services;
    }
    
}
