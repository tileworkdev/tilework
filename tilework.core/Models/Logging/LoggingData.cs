namespace Tilework.Logging.Models;

public class LoggingData
{
    public DateTimeOffset Timestamp { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string> Labels { get; set; } = new();
}
