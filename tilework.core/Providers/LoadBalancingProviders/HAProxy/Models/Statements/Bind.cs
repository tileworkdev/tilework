namespace Tilework.LoadBalancing.Haproxy;

public class Bind
{
    public string Address { get; set; }
    public int Port { get; set; }
    
    public bool EnableTls { get; set; }

    public Bind() { }

    public Bind(string [] parameters)
    {

    }

    public override string ToString()
    {
        if(EnableTls)
            return $"{Address}:{Port} ssl crt-list /usr/local/etc/haproxy/certs/certlist.txt";
        else
            return $"{Address}:{Port}";
    }
}