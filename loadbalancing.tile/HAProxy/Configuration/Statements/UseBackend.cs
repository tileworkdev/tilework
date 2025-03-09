namespace Tilework.LoadBalancing.Haproxy;

public class UseBackend
{
    public string Hostname { get; set; }
    public string Target { get; set; }

    public UseBackend() {}

    public UseBackend(string [] parameters)
    {

    }

    public override string ToString()
    {
        return $"{Target} if {{ hdr(host) -i {Hostname} }}";
    }
}