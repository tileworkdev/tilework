namespace Tilework.Monitoring.Interfaces;

public interface IDataCollectorConfigurator
{
    Task ApplyConfiguration(List<Monitoring.Models.Monitor> monitors);
    Task Shutdown();
}