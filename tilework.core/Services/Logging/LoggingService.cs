using Tilework.Logging.Interfaces;
using Tilework.Logging.Models;

namespace Tilework.Logging.Services;

public class LoggingService
{
    private readonly ILoggingDataPersistenceConfigurator _persistenceConfigurator;

    public LoggingService(ILoggingDataPersistenceConfigurator persistenceConfigurator)
    {
        _persistenceConfigurator = persistenceConfigurator;
    }

    public Task<List<LoggingData>> GetLoggingData(
        string module,
        Dictionary<string, string> filters,
        DateTimeOffset start,
        DateTimeOffset end)
    {
        return _persistenceConfigurator.GetData(module, filters, start, end);
    }
}
