using Tilework.Logging.Models;

namespace Tilework.Logging.Interfaces;

public interface ILoggingDataPersistenceConfigurator
{
    Task<LoggingTarget> GetTarget();
    Task<List<LoggingData>> GetData(string module,
                                    Dictionary<string, string> filters,
                                    DateTimeOffset start,
                                    DateTimeOffset end);
    Task ApplyConfiguration();
    Task Shutdown();
}
