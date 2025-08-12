namespace Tilework.LoadBalancing.Haproxy;

public class FrontendSection : ConfigSection
{
    [Statement("bind")]
    public Bind Bind { get; set; }

    [Statement("mode")]
    public Mode Mode { get; set; }

    [Statement("acl")]
    public List<Acl> Acls { get; set; } = new List<Acl>();

    [Statement("use_backend")]
    public List<UseBackend> UseBackends { get; set; } = new List<UseBackend>();

    [Statement("default_backend")]
    public string DefaultBackend { get; set; }

    public FrontendSection() : base("frontend")
    {
    }
}