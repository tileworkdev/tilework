using Tilework.LoadBalancing.Enums;

namespace Tilework.LoadBalancing.Haproxy;

public class Acl
{
    public string Name { get; set; }
    public AclCondition Type { get; set; }
    public List<string> Values { get; set; } = new List<string>();

    public Acl() {}

    public Acl(string [] parameters)
    {

    }

    public override string ToString()
    {
        return Type switch
        {
            AclCondition.HostHeader => $"{Name} hdr(host) -i {string.Join(" ", Values)}",
            AclCondition.Path => $"{Name} path_beg -i {string.Join(" ", Values)}",
            AclCondition.QueryString => $"{Name} {String.Join(" or ", Values.Select(v => $"url_param(plan) -i {v}"))}",
            AclCondition.SNI => $"{Name} req.ssl_sni -i {string.Join(" ", Values)}",
            AclCondition.SourceIp => $"{Name} src {string.Join(" ", Values)}",
            AclCondition.VariableSet => $"{Name} {String.Join(" or ", Values.Select(v => $"var({v}) -m found"))}",
        };
    }
}
