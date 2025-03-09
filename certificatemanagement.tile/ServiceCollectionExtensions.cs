using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

using Tilework.CertificateManagement.Settings;
using Tilework.CertificateManagement.Persistence;

namespace Tilework.CertificateManagement.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCertificateManagement(this IServiceCollection services,
                                                              IConfiguration configuration,
                                                              Action<DbContextOptionsBuilder> dbContextOptions)
    {
        services.Configure<CertificateManagementSettings>(configuration);

        services.AddScoped<CertificateManagementService>();

        services.AddDbContext<CertificateManagementContext>(dbContextOptions);

        return services;
    }
}
