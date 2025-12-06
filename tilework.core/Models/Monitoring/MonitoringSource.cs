using Tilework.Core.Models;
using Tilework.Monitoring.Enums;

namespace Tilework.Monitoring.Models;
public class MonitoringSource
{
    public string Module { get; set; }
    public string Name { get; set; }
    public MonitoringSourceType Type { get; set; }

    public Host Host { get; set; }
    public int Port { get; set; }

    public string? Username { get; set; }
    public string? Password { get; set; }
}