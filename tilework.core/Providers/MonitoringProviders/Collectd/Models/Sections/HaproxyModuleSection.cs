namespace Tilework.Monitoring.Collectd;

public class HaproxyModuleSection : ModuleSection
{
    public HaproxyModuleSection()
    {
    }

    [Statement("Socket")]
    public string? Socket { get; set;  }

    [Statement("Endpoint")]
    public string? Endpoint { get; set; }
}