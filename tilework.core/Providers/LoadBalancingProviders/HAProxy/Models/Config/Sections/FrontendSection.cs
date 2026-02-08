namespace Tilework.LoadBalancing.Haproxy;

public class FrontendSection : ConfigSection
{
    [Statement("bind")]
    public Bind Bind { get; set; }

    [Statement("mode")]
    public Mode Mode { get; set; }

    [Statement("acl")]
    public List<Acl> Acls { get; set; } = new List<Acl>();

    [Statement("http-request")]
    public List<HttpRequest> HttpRequests { get; set; } = new List<HttpRequest>();

    [Statement("tcp-request")]
    public List<TcpRequest> TcpRequests { get; set; } = new List<TcpRequest>();

    [Statement("use_backend")]
    public List<UseBackend> UseBackends { get; set; } = new List<UseBackend>();

    [Statement("default_backend")]
    public string DefaultBackend { get; set; }

    [Statement("http-request add-header")]
    public List<HttpHeader> AddHeaders { get; set; } = new List<HttpHeader>();

    public FrontendSection() : base("frontend")
    {
    }
}
