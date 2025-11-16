using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;


using Tilework.Monitoring.Models;
using Tilework.Core.Persistence;
using Tilework.Monitoring.Interfaces;

namespace Tilework.LoadBalancing.Services;

public class DataCollectorService
{
    private readonly IDataCollectorConfigurator _collectorConfigurator;
    private readonly IDataPersistenceConfigurator _persistenceConfigurator;
    private readonly TileworkContext _dbContext;
    private readonly DataCollectorConfiguration _settings;
    private readonly ILogger<DataCollectorService> _logger;

    private List<MonitoringSource> _sources = new();

    public DataCollectorService(IDataCollectorConfigurator collectorConfigurator,
                                IDataPersistenceConfigurator persistenceConfigurator,
                                TileworkContext dbContext,
                                IOptions<DataCollectorConfiguration> settings,
                                ILogger<DataCollectorService> logger)
    {
        _collectorConfigurator = collectorConfigurator;
        _persistenceConfigurator = persistenceConfigurator;
        _dbContext = dbContext;
        _logger = logger;
        _settings = settings.Value;
    }

    public bool IsMonitored(string name)
    {
        return _sources.Any(s => s.Name == name);
    }

    public List<MonitoringSource> GetMonitoredSources()
    {
        return _sources;
    }

    public async Task StartMonitoring(MonitoringSource source)
    {
        if (!IsMonitored(source.Name))
            _sources.Add(source);

        await ApplyConfiguration();
    }

    public async Task StopMonitoring(string name)
    {
        if (IsMonitored(name))
        {
            var source = _sources.First(s => s.Name == name);
            _sources.Remove(source);
        }

        await ApplyConfiguration();
    }

    public async Task ApplyConfiguration()
    {
        await _persistenceConfigurator.ApplyConfiguration();

        var monitors = (await Task.WhenAll(
            _sources.Select(async s => new Monitoring.Models.Monitor
            {
                Source = s,
                Target = await _persistenceConfigurator.GetTarget(s)
            })
        )).ToList();

        await _collectorConfigurator.ApplyConfiguration(monitors);
    }

    public async Task Shutdown()
    {
        await _persistenceConfigurator.Shutdown();
        await _collectorConfigurator.Shutdown();
    }
}