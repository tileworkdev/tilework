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
        var normalized = NormalizeHost(input);
        if (IsValidHostname(normalized))
        { result = new Host(normalized); return true; }

        result = default; return false;
    }

    public static Host Parse(string input) =>
        TryParse(input, out var v) ? v : throw new FormatException("Invalid host/IP.");

    static string NormalizeIp(string s) => s;
    static string NormalizeHost(string s)
    {
        s = s.Trim().TrimEnd('.').ToLowerInvariant();
        // IDN -> punycode
        var idn = new System.Globalization.IdnMapping();
        var labels = s.Split('.');
        for (int i = 0; i < labels.Length; i++) labels[i] = idn.GetAscii(labels[i]);
        return string.Join('.', labels);
    }
    static bool IsValidHostname(string h)
    {
        if (h.Length is 0 or > 253) return false;
        foreach (var label in h.Split('.'))
        {
            if (label.Length is 0 or > 63) return false;
            if (label.StartsWith('-') || label.EndsWith('-')) return false;
            foreach (var ch in label)
                if (!(ch is >= 'a' and <= 'z' || ch is >= '0' and <= '9' || ch == '-')) return false;
        }
        return true;
    }
}