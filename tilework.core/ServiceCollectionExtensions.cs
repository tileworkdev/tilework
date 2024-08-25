using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

using Tilework.Core.Interfaces;

namespace Tilework.Core.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<IServiceManager, SystemdServiceManager>();
        return services;
    }
}
