using Tilework.Logging.Models;

namespace Tilework.Logging.Interfaces;

public interface ILoggingDataCollectorConfigurator
{
    Task ApplyConfiguration(List<LoggingSource> sources, LoggingTarget target);
    Task Shutdown();
}
