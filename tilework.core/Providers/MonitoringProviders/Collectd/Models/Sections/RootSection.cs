namespace Tilework.Monitoring.Collectd;

public class RootSection : ConfigSection
{
    [Statement("Hostname")]
    public string Hostname { get; set; }

    [Statement("BaseDir")]
    public string BaseDir { get; set; }

    [Statement("PIDFile")]
    public string PidFile { get; set; }

    [Statement("PluginDir")]
    public string PluginDir { get; set; }

    [Statement("TypesDB")]
    public string TypesDb { get; set; }

    public RootSection() : base("")
    {
    }
}

