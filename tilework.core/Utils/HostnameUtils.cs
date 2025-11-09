using System.Globalization;

namespace Tilework.Core.Utils;

public static class HostnameUtils
{
    public static string NormalizeHost(string s)
    {
        s = s.Trim().TrimEnd('.').ToLowerInvariant();
        var idn = new IdnMapping();
        var labels = s.Split('.');
        for (int i = 0; i < labels.Length; i++) labels[i] = idn.GetAscii(labels[i]);
        return string.Join('.', labels);
    }

    public static bool IsValidHostname(string h)
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
