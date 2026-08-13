using System.Net;
using System.Net.Http;

using Tilework.LoadBalancing.Enums;

namespace Tilework.LoadBalancing.Models;

public record class LoadBalancerLogEntry
{
    public DateTimeOffset Timestamp { get; init; }
    public LoadBalancerLogAction Action { get; init; }
    public string? Backend { get; init; }
    public IPAddress SourceAddress { get; init; } = IPAddress.None;
    public HttpStatusCode StatusCode { get; init; }
    public HttpMethod Method { get; init; } = HttpMethod.Get;
    public string Path { get; init; } = string.Empty;
    public long TotalTimeMilliseconds { get; init; }
}
