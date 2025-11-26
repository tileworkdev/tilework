using Microsoft.Extensions.Logging;

using Tilework.Monitoring.Interfaces;

public class MonitoringService
{
    private readonly IDataPersistenceConfigurator _persistenceConfigurator;

    private readonly ILogger<MonitoringService> _logger;


    public MonitoringService(IDataPersistenceConfigurator persistenceConfigurator,
                             ILogger<MonitoringService> logger)
    {
        _persistenceConfigurator = persistenceConfigurator;
        _logger = logger;
    }

    public async Task<List<T>> GetMonitoringData<T>(string monitorName, DateTimeOffset start, DateTimeOffset end) where T : BaseMonitorData, new()
    {
        return await _persistenceConfigurator.GetData<T>(monitorName, start, end);
    }
}