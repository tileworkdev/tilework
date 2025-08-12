using Tilework.LoadBalancing.Enums;

namespace Tilework.LoadBalancing.Haproxy;

public class Acl
{
    public string Name { get; set; }
    public ConditionType Type { get; set; }
    public List<string> Values { get; set; } = new List<string>();

    public Acl() {}

    public Acl(string [] parameters)
    {

    }

    public override string ToString()
    {
        return Type switch
        {
            ConditionType.HostHeader => $"{Name} hdr(host) -i {string.Join(" ", Values)}",
            ConditionType.Path => $"{Name} path_beg -i {string.Join(" ", Values)}",
            ConditionType.QueryString => $"{Name} {String.Join(" or ", Values.Select(v => $"url_param(plan) -i {v}"))}"
        };
    }
}