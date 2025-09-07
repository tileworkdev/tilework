using Tilework.Persistence.LoadBalancing.Models;

namespace Tilework.Monitoring.Interfaces;

public interface IDataCollectorConfigurator
{
    string ServiceName { get; }

    Task ApplyConfiguration(List<MonitoringSource> sources);
    Task Shutdown();
}