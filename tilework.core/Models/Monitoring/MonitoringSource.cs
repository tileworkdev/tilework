using Tilework.Core.Models;
using Tilework.Monitoring.Enums;

public class MonitoringSource
{
    public string Name { get; set; }
    public MonitoringSourceType Type { get; set; }

    public Host Host { get; set; }
    public int Port { get; set; }

    public string? Username { get; set; }
    public string? Password { get; set; }
}