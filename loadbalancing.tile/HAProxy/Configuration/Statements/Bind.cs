namespace Tilework.LoadBalancing.Haproxy;

public class Bind
{
    public string Address { get; set; }
    public int Port { get; set; }

    public Bind() {}

    public Bind(string [] parameters)
    {

    }

    public override string ToString()
    {
        return $"{Address}:{Port}";
    }
}