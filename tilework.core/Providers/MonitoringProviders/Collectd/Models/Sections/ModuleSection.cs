namespace Tilework.Monitoring.Collectd;

public class ModuleSection : ConfigSection
{
    public ModuleSection() : base("Module", false)
    {
    }

    [Statement("Instance")]
    public string Instance { get; set; }
}