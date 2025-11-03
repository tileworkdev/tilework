using Tilework.Monitoring.Models;

namespace Tilework.Monitoring.Interfaces;

public interface IDataPersistenceConfigurator
{
    string ServiceName { get; }
    Task<MonitoringTarget> GetTarget(MonitoringSource source);
    Task ApplyConfiguration();
    Task Shutdown();
}