using Tilework.Core.Models;

namespace Tilework.Logging.Models;

public class LoggingTarget
{
    public string Name { get; set; } = string.Empty;
    public Host Host { get; set; }
    public int Port { get; set; }
}
