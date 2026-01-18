namespace Tilework.LoadBalancing.Haproxy;

public class Server
{
    public string Name { get; set; }
    public string Address { get; set; }
    public int Port { get; set; }
    public bool Check { get; set; }
    public bool Tls { get; set; }

    public Server() {}

    public Server(string [] parameters)
    {
        Name = parameters[1];

        string[] parts = parameters[2].Split(':');
        Address = parts[0];
        Port = Int32.Parse(parts[1]);

    }

    public override string ToString()
    {
        var checkStr = Check == true ? "check" : "";
        var tlsStr = Tls == true ? "ssl verify required" : "";
        return $"{Name} {Address}:{Port} {tlsStr} {checkStr}";
    }
}