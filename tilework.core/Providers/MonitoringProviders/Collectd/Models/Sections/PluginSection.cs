namespace Tilework.Monitoring.Collectd;

public class PluginSection : ConfigSection
{
    public PluginSection() : base("Plugin")
    {
    }

    public List<ModuleSection> Modules { get; set; } = new();

    [Statement("Import")]
    public List<string> Imports { get; set; }
}
