using System.Linq;
using Tilework.LoadBalancing.Enums;

namespace Tilework.LoadBalancing.Haproxy;

public class HttpRequest
{
    public RuleActionType ActionType { get; set; }
    public string? RedirectUrl { get; set; }
    public int? RedirectStatusCode { get; set; }
    public int? FixedResponseStatusCode { get; set; }
    public string? FixedResponseContentType { get; set; }
    public string? FixedResponseBody { get; set; }
    public List<string> Acls { get; set; } = new();

    public HttpRequest() { }

    public HttpRequest(string[] parameters)
    {
    }

    public override string ToString()
    {
        return ActionType switch
        {
            RuleActionType.Redirect => BuildRedirect(),
            RuleActionType.FixedResponse => BuildReturn(),
            _ => throw new NotSupportedException($"Unsupported HTTP action type: {ActionType}")
        };
    }

    private string BuildRedirect()
    {
        var parts = new List<string> { "redirect", "location", RedirectUrl ?? string.Empty };

        if (RedirectStatusCode.HasValue)
        {
            parts.Add("code");
            parts.Add(RedirectStatusCode.Value.ToString());
        }

        if (Acls != null && Acls.Count > 0)
        {
            parts.Add("if");
            parts.AddRange(Acls);
        }

        return string.Join(" ", parts);
    }

    private string BuildReturn()
    {
        var parts = new List<string>
        {
            "return",
            "status",
            FixedResponseStatusCode!.ToString()
        };

        if (!string.IsNullOrWhiteSpace(FixedResponseContentType))
        {
            parts.Add("content-type");
            parts.Add(FixedResponseContentType);
        }

        if (!string.IsNullOrWhiteSpace(FixedResponseBody))
        {
            parts.Add("lf-string");
            parts.Add(Quote(FixedResponseBody));
        }

        if (Acls != null && Acls.Count > 0)
        {
            parts.Add("if");
            parts.AddRange(Acls);
        }

        return string.Join(" ", parts);
    }

    private void ParseRedirect(string[] parameters)
    {
        RedirectUrl = GetValueAfter(parameters, "location");
        var code = GetValueAfter(parameters, "code");
        if (int.TryParse(code, out var parsed))
        {
            RedirectStatusCode = parsed;
        }
        Acls = GetAcls(parameters);
    }

    private void ParseReturn(string[] parameters)
    {
        var status = GetValueAfter(parameters, "status");
        if (int.TryParse(status, out var parsed))
        {
            FixedResponseStatusCode = parsed;
        }
        FixedResponseContentType = GetValueAfter(parameters, "content-type");
        var body = GetValueAfter(parameters, "lf-string");
        if (!string.IsNullOrWhiteSpace(body))
        {
            FixedResponseBody = body.Trim('"');
        }
        Acls = GetAcls(parameters);
    }

    private static string? GetValueAfter(string[] parameters, string token)
    {
        for (int i = 0; i < parameters.Length - 1; i++)
        {
            if (string.Equals(parameters[i], token, StringComparison.OrdinalIgnoreCase))
            {
                return parameters[i + 1];
            }
        }

        return null;
    }

    private static List<string> GetAcls(string[] parameters)
    {
        for (int i = 0; i < parameters.Length; i++)
        {
            if (string.Equals(parameters[i], "if", StringComparison.OrdinalIgnoreCase))
            {
                return parameters.Skip(i + 1).ToList();
            }
        }

        return new List<string>();
    }

    private static string Quote(string value)
    {
        var escaped = value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", string.Empty)
            .Replace("\n", "\\n");
        return $"\"{escaped}\"";
    }
}
