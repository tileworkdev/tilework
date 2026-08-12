using Tilework.Logging.Models;

namespace Tilework.Logging.Interfaces;

public interface ILoggingDataPersistenceConfigurator
{
    Task<LoggingTarget> GetTarget();
    Task ApplyConfiguration();
    Task Shutdown();
}
