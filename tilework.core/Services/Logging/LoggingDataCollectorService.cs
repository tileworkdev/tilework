using Tilework.Logging.Interfaces;
using Tilework.Logging.Models;

namespace Tilework.Logging.Services;

public class LoggingDataCollectorService
{
    private readonly ILoggingDataCollectorConfigurator _collectorConfigurator;
    private readonly ILoggingDataPersistenceConfigurator _persistenceConfigurator;
    private readonly SemaphoreSlim _configurationLock = new(1, 1);
    private readonly List<LoggingSource> _sources = new();

    public LoggingDataCollectorService(
        ILoggingDataCollectorConfigurator collectorConfigurator,
        ILoggingDataPersistenceConfigurator persistenceConfigurator)
    {
        _collectorConfigurator = collectorConfigurator;
        _persistenceConfigurator = persistenceConfigurator;
    }

    public bool IsLogging(string name)
    {
        lock (_sources)
            return _sources.Any(source => source.Name == name);
    }

    public List<LoggingSource> GetLoggingSources()
    {
        lock (_sources)
            return _sources.Select(CopySource).ToList();
    }

    public async Task StartLogging(LoggingSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        ValidateSource(source);

        await _configurationLock.WaitAsync();
        try
        {
            lock (_sources)
            {
                var existing = _sources.FindIndex(item => item.Name == source.Name);
                if (existing >= 0)
                    _sources[existing] = CopySource(source);
                else
                    _sources.Add(CopySource(source));
            }

            await ApplyConfigurationCore();
        }
        finally
        {
            _configurationLock.Release();
        }
    }

    public async Task StopLogging(string name)
    {
        await _configurationLock.WaitAsync();
        try
        {
            lock (_sources)
                _sources.RemoveAll(source => source.Name == name);

            await ApplyConfigurationCore();
        }
        finally
        {
            _configurationLock.Release();
        }
    }

    public async Task StopLoggingForContainer(string containerName)
    {
        await _configurationLock.WaitAsync();
        try
        {
            int removed;
            lock (_sources)
                removed = _sources.RemoveAll(source => source.ContainerName == containerName);

            if (removed > 0)
                await ApplyConfigurationCore();
        }
        finally
        {
            _configurationLock.Release();
        }
    }

    public async Task ApplyConfiguration()
    {
        await _configurationLock.WaitAsync();
        try
        {
            await ApplyConfigurationCore();
        }
        finally
        {
            _configurationLock.Release();
        }
    }

    public async Task Shutdown()
    {
        await _configurationLock.WaitAsync();
        try
        {
            lock (_sources)
                _sources.Clear();

            await _collectorConfigurator.Shutdown();
            await _persistenceConfigurator.Shutdown();
        }
        finally
        {
            _configurationLock.Release();
        }
    }

    private async Task ApplyConfigurationCore()
    {
        await _persistenceConfigurator.ApplyConfiguration();
        var target = await _persistenceConfigurator.GetTarget();
        await _collectorConfigurator.ApplyConfiguration(GetLoggingSources(), target);
    }

    private static void ValidateSource(LoggingSource source)
    {
        if (string.IsNullOrWhiteSpace(source.Module))
            throw new ArgumentException("A logging source module is required", nameof(source));
        if (string.IsNullOrWhiteSpace(source.Name))
            throw new ArgumentException("A logging source name is required", nameof(source));
        if (string.IsNullOrWhiteSpace(source.ContainerId))
            throw new ArgumentException("A logging source container ID is required", nameof(source));
        if (string.IsNullOrWhiteSpace(source.ContainerName))
            throw new ArgumentException("A logging source container name is required", nameof(source));
    }

    private static LoggingSource CopySource(LoggingSource source) => new()
    {
        Module = source.Module,
        Name = source.Name,
        ContainerId = source.ContainerId,
        ContainerName = source.ContainerName
    };
}
