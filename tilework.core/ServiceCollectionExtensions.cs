using Microsoft.Extensions.DependencyInjection;

using Tilework.Core.Interfaces;

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
}
