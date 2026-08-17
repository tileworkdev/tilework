using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

using Tilework.LoadBalancing.Enums;
using Tilework.LoadBalancing.Models;
using Tilework.Logging.Models;

namespace Tilework.LoadBalancing.Haproxy;

public static partial class HAProxyLogParser
{
    public static bool TryParse(LoggingData data, out LoadBalancerLogEntry? entry)
    {
        entry = null;

        var match = HttpLogPattern().Match(data.Message.Trim());
        if (!match.Success ||
            !IPAddress.TryParse(match.Groups["sourceAddress"].Value, out var sourceAddress) ||
            !DateTimeOffset.TryParseExact(
                match.Groups["timestamp"].Value,
                "dd/MMM/yyyy:HH:mm:ss.fff",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var timestamp) ||
            !int.TryParse(match.Groups["statusCode"].Value, out var statusCode) ||
            !long.TryParse(match.Groups["totalTime"].Value, out var totalTime) ||
            totalTime < 0)
        {
            return false;
        }

        var frontend = match.Groups["frontend"].Value.TrimEnd('~', '+');
        var backend = match.Groups["backend"].Value;
        var server = match.Groups["server"].Value;
        var method = new HttpMethod(match.Groups["method"].Value);
        var capturedHeaders = CapturedRequestHeadersPattern().Match(match.Groups["logDetails"].Value);
        var wasBackendSelected = !string.Equals(frontend, backend, StringComparison.Ordinal);
        var wasServerSelected = !string.Equals(server, "<NOSRV>", StringComparison.Ordinal);

        entry = new LoadBalancerLogEntry
        {
            Timestamp = timestamp,
            Action = wasServerSelected
                ? LoadBalancerLogAction.Forwarded
                : wasBackendSelected
                    ? LoadBalancerLogAction.ForwardingFailed
                    : LoadBalancerLogAction.RespondedByLoadBalancer,
            Backend = wasBackendSelected ? backend : null,
            SourceAddress = sourceAddress,
            StatusCode = (HttpStatusCode)statusCode,
            Method = method,
            Path = match.Groups["path"].Value,
            HostHeader = GetCapturedHeader(capturedHeaders, "hostHeader"),
            UserAgent = GetCapturedHeader(capturedHeaders, "userAgent"),
            XForwardedFor = GetCapturedHeader(capturedHeaders, "xForwardedFor"),
            TotalTimeMilliseconds = totalTime
        };

        return true;
    }

    private static string? GetCapturedHeader(Match capturedHeaders, string groupName)
    {
        var group = capturedHeaders.Groups[groupName];
        return group.Success && !string.IsNullOrEmpty(group.Value) ? group.Value : null;
    }

    [GeneratedRegex(
        """^(?<sourceAddress>.+):\d+ \[(?<timestamp>[^\]]+)\] (?<frontend>\S+) (?<backend>\S+)/(?<server>\S+) -?\d+/-?\d+/-?\d+/-?\d+/(?<totalTime>-?\d+) (?<statusCode>\d{3}) \d+ (?<logDetails>.*)"(?<method>[!#$%&'*+.^_`|~0-9A-Za-z-]+) (?<path>.*?) HTTP/\d(?:\.\d+)?"$""",
        RegexOptions.CultureInvariant)]
    private static partial Regex HttpLogPattern();

    [GeneratedRegex(
        @"\{(?<hostHeader>[^|}]*)\|(?<userAgent>[^|}]*)\|(?<xForwardedFor>[^}]*)\}",
        RegexOptions.CultureInvariant)]
    private static partial Regex CapturedRequestHeadersPattern();
}
