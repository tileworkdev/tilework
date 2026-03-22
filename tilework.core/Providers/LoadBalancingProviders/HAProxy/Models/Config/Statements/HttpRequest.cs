namespace Tilework.LoadBalancing.Haproxy;

public abstract class HttpRequest
{
    public List<string> Acls { get; set; } = new();
    public abstract HttpRequestAction Action { get; }

    public override string ToString()
    {
        var parts = BuildParts();

        if (Acls.Count > 0)
        {
            parts.Add("if");
            parts.AddRange(Acls);
        }

        return string.Join(" ", parts);
    }

    protected abstract List<string> BuildParts();

    protected static string Quote(string value)
    {
        var escaped = value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", string.Empty)
            .Replace("\n", "\\n");
        return $"\"{escaped}\"";
    }
}

public sealed class RedirectHttpRequest(string url) : HttpRequest
{
    public string Url { get; set; } = url;
    public int? StatusCode { get; set; }
    public override HttpRequestAction Action => HttpRequestAction.Redirect;

    protected override List<string> BuildParts()
    {
        var parts = new List<string> { "redirect", "location", Url };

        if (StatusCode.HasValue)
        {
            parts.Add("code");
            parts.Add(StatusCode.Value.ToString());
        }

        return parts;
    }
}

public sealed class ReturnHttpRequest(int statusCode) : HttpRequest
{
    public int StatusCode { get; set; } = statusCode;
    public string? ContentType { get; set; }
    public string? Body { get; set; }
    public override HttpRequestAction Action => HttpRequestAction.Return;

    protected override List<string> BuildParts()
    {
        var parts = new List<string> { "return", "status", StatusCode.ToString() };

        if (!string.IsNullOrWhiteSpace(ContentType))
        {
            parts.Add("content-type");
            parts.Add(ContentType);
        }

        if (!string.IsNullOrWhiteSpace(Body))
        {
            parts.Add("lf-string");
            parts.Add(Quote(Body));
        }

        return parts;
    }
}

public sealed class SetVariableHttpRequest(string variableName, string variableValue) : HttpRequest
{
    public string VariableName { get; set; } = variableName;
    public string VariableValue { get; set; } = variableValue;
    public override HttpRequestAction Action => HttpRequestAction.SetVariable;

    protected override List<string> BuildParts()
    {
        return new List<string> { $"set-var({VariableName}) str({VariableValue})" };
    }
}

public sealed class DenyHttpRequest : HttpRequest
{
    public override HttpRequestAction Action => HttpRequestAction.Deny;

    protected override List<string> BuildParts()
    {
        return new List<string> { "deny" };
    }
}
