using Tilework.Ui.Mappers;
using Tilework.Ui.Services;

namespace Tilework.Ui.ViewModels;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserInterface(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(FormMappingProfile));
        services.AddScoped<IBrowserTimeZoneProvider, BrowserTimeZoneProvider>();

        return services;
    }
}
