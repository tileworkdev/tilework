using Tilework.Core.Utils;

namespace Tilework.Core.Models;

public readonly record struct Host(string Value)
{
    public static bool TryParse(string input, out Host result)
    {
        if (string.IsNullOrWhiteSpace(input)) { result = default; return false; }

        // IP?
        if (System.Net.IPAddress.TryParse(input, out _))
        { result = new Host(NormalizeIp(input)); return true; }

        // DNS name?
        var normalized = HostnameUtils.NormalizeHost(input);
        if (HostnameUtils.IsValidHostname(normalized))
        { result = new Host(normalized); return true; }

        result = default; return false;
    }

    public static Host Parse(string input) =>
        TryParse(input, out var v) ? v : throw new FormatException("Invalid host/IP.");

    static string NormalizeIp(string s) => s;
}
