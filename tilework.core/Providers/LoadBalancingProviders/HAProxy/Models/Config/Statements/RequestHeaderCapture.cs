namespace Tilework.LoadBalancing.Haproxy;

public sealed class RequestHeaderCapture
{
    public string Name { get; set; }
    public int Length { get; set; }

    public RequestHeaderCapture(string name, int length)
    {
        Name = name;
        Length = length;
    }

    public RequestHeaderCapture(string[] parameters)
    {
        if (parameters.Length != 6 ||
            !string.Equals(parameters[0], "capture", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(parameters[1], "request", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(parameters[2], "header", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(parameters[4], "len", StringComparison.OrdinalIgnoreCase))
        {
            throw new FormatException("Invalid HAProxy request-header capture statement.");
        }

        Name = parameters[3];
        Length = int.Parse(parameters[5]);
    }

    public override string ToString()
    {
        return $"request header {Name} len {Length}";
    }
}
