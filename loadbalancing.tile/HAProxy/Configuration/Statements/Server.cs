namespace Tilework.LoadBalancing.Haproxy;

public class Server
{
    public string Name { get; set; }
    public string Address { get; set; }
    public int Port { get; set; }

    public Server() {}

    public Server(string [] parameters)
    {

    }

    public override string ToString()
    {
        return $"{Name} {Address}:{Port}";
    }
}