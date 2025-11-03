using Tilework.Core.Models;
using Tilework.Monitoring.Enums;

namespace Tilework.Monitoring.Models;

public class MonitoringTarget
{
    public string Name { get; set; }
    public MonitoringPersistenceType Type { get; set; }

    public Host Host { get; set; }
    public int Port { get; set; }

    public string? Username { get; set; }
    public string? Password { get; set; }
}