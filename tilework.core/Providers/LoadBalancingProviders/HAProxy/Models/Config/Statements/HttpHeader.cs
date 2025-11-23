namespace Tilework.LoadBalancing.Haproxy;

public class HttpHeader
{
    public string Name { get; set; }
    public string Value { get; set; }

    public HttpHeader() {}

    public HttpHeader(string [] parameters)
    {

    }

    public override string ToString()
    {
        return $"{Name} {Value}";
    }
}