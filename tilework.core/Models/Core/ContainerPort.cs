using Tilework.Core.Enums;

namespace Tilework.Core.Models;

public class ContainerPort
{
    public int Port { get; set; }
    public int? HostPort { get; set; }
    public PortType Type { get; set; }
}