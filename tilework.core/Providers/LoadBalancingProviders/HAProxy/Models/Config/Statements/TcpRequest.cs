using System.Linq;

namespace Tilework.LoadBalancing.Haproxy;

public class TcpRequest
{
    public List<string> Acls { get; set; } = new();

    public TcpRequest() { }

    public TcpRequest(string[] parameters)
    {
    }

    public override string ToString()
    {
        if (Acls == null || Acls.Count == 0)
        {
            return "connection reject";
        }

        return $"connection reject if {string.Join(" ", Acls)}";
    }
}
