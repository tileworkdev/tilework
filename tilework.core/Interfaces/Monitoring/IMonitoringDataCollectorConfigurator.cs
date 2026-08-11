namespace Tilework.Monitoring.Interfaces;

public interface IMonitoringDataCollectorConfigurator
{
    Task ApplyConfiguration(List<Monitoring.Models.Monitor> monitors);
    Task Shutdown();
}
