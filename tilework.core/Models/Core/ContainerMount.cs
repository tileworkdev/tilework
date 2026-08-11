namespace Tilework.Core.Models;

public class ContainerMount
{
    public string Source { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public bool ReadOnly { get; set; }
}
