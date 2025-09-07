using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;


using Tilework.Monitoring.Models;
using Tilework.Core.Persistence;
using Tilework.Monitoring.Interfaces;

namespace Tilework.LoadBalancing.Services;

public class DataCollectorService
{
    private readonly IDataCollectorConfigurator _configurator;
    private readonly TileworkContext _dbContext;
    private readonly DataCollectorConfiguration _settings;
    private readonly ILogger<DataCollectorService> _logger;

    private List<MonitoringSource> _sources = new();

    public DataCollectorService(IDataCollectorConfigurator configurator,
                                TileworkContext dbContext,
                                IOptions<DataCollectorConfiguration> settings,
                                ILogger<DataCollectorService> logger)
    {
        _configurator = configurator;
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
        await _configurator.ApplyConfiguration(_sources);
    }

    public async Task Shutdown()
    {
        await _configurator.Shutdown();
    }
}