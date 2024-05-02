namespace Tilework.LoadBalancing.Haproxy;

public class FrontendSection : ConfigSection
{
    [Statement("bind")]
    public Bind Bind { get; set; }

    [Statement("mode")]
    public Mode Mode { get; set; }

    [Statement("default_backend")]
    public string DefaultBackend { get; set; }

    public FrontendSection() : base("frontend")
    {
    }
}