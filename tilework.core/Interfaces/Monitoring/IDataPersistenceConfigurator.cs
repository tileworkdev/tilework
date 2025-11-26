using Tilework.Monitoring.Models;

namespace Tilework.Monitoring.Interfaces;

public interface IDataPersistenceConfigurator
{
    string ServiceName { get; }
    Task<MonitoringTarget> GetTarget(MonitoringSource source);
    Task<List<T>> GetData<T>(string name, DateTimeOffset start, DateTimeOffset end) where T : BaseMonitorData, new();

    Task ApplyConfiguration();
    Task Shutdown();
}