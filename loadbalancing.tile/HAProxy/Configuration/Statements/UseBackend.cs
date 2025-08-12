namespace Tilework.LoadBalancing.Haproxy;

public class UseBackend
{
    public List<string> Acls { get; set; }
    public string Target { get; set; }

    public UseBackend() {}

    public UseBackend(string [] parameters)
    {

    }

    public override string ToString()
    {
        return $"{Target} if {string.Join(" ", Acls)}";
    }
}