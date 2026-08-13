using Tilework.Core.Models;
using Tilework.Logging.Enums;

namespace Tilework.Logging.Models;

public class LoggingTarget
{
    public string Name { get; set; } = string.Empty;
    public LoggingPersistenceType Type { get; set; }
    public Host Host { get; set; }
    public int Port { get; set; }
}
