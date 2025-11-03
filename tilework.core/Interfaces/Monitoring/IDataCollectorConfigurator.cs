namespace Tilework.Monitoring.Interfaces;

public interface IDataCollectorConfigurator
{
    string ServiceName { get; }

    Task ApplyConfiguration(List<Monitoring.Models.Monitor> monitors);
    Task Shutdown();
}