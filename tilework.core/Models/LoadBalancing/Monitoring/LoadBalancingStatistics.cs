using System.Reflection;
using Tilework.Core.Attributes;

namespace Tilework.LoadBalancing.Models;

public class LoadBalancingStatistics
{
    public TimeSpan Uptime { get; set; }
    // public int? CurrentSessions { get; init; }
    // public int? CurrentQueue { get; init; }



    // // Queue

    // public int? MaxQueue { get; init; }
    // public int? QueueLimit { get; init; }

    // // Sessions / connections

    // public int? MaxSessions { get; init; }
    // public int? SessionLimit { get; init; }
    [Cumulative]
    public long? TotalSessions { get; set; }

    [Cumulative]
    public long? BytesIn { get; set; }
    [Cumulative]
    public long? BytesOut { get; set; }


    [Cumulative]
    public long? DeniedRequests { get; init; }
    [Cumulative]
    public long? DeniedResponses { get; init; }
    [Cumulative]
    public long? RequestErrors { get; init; }
    [Cumulative]
    public long? ConnectionErrors { get; init; }
    [Cumulative]
    public long? ResponseErrors { get; init; }
    // public long? Retries { get; init; }
    // public long? Redispatches { get; init; }

    // // HTTP responses
    [Cumulative]
    public long? Responses1xx { get; set; }
    [Cumulative]
    public long? Responses2xx { get; set; }
    [Cumulative]
    public long? Responses3xx { get; set; }
    [Cumulative]
    public long? Responses4xx { get; set; }
    [Cumulative]
    public long? Responses5xx { get; set; }
    // public long? ResponsesOther { get; init; }


    public int? AvgQueueTimeMs { get; set; }
    public int? AvgConnectTimeMs { get; set; }
    public int? AvgResponseTimeMs { get; set; }
    public int? AvgTotalTimeMs { get; set; }

    // // Status / health
    // public string? Status { get; init; }
    // public int? Weight { get; init; }
    // public int? ActiveServers { get; init; }
    // public int? BackupServers { get; init; }
    // public long? FailedChecks { get; init; }
    // public long? DowntimeTransitions { get; init; }
    // public int? SecondsSinceLastChange { get; init; }
    // public long? TotalDowntime { get; init; }

    // // Compression
    // public long? CompressedIn { get; init; }
    // public long? CompressedOut { get; init; }
    // public long? CompressionBypassed { get; init; }

    // // Cache
    // public long? CacheHits { get; init; }
    // public long? CacheMisses { get; init; }

    public static LoadBalancingStatistics operator -(LoadBalancingStatistics a, LoadBalancingStatistics b)
    {
        var result = new LoadBalancingStatistics();

        foreach (var prop in typeof(LoadBalancingStatistics).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead || !prop.CanWrite) continue;

            var aVal = prop.GetValue(a);
            var bVal = prop.GetValue(b);

            if (prop.IsDefined(typeof(CumulativeAttribute), inherit: true))
            {
                object? diff = null;

                if (prop.PropertyType == typeof(TimeSpan))
                    diff = (TimeSpan)aVal - (TimeSpan)bVal;
                else if (prop.PropertyType == typeof(long?))
                    diff = (aVal is long la && bVal is long lb) ? la - lb : null;
                else if (prop.PropertyType == typeof(int?))
                    diff = (aVal is int ia && bVal is int ib) ? ia - ib : null;
                else if (prop.PropertyType == typeof(double?))
                    diff = (aVal is double da && bVal is double db) ? da - db : null;
                else if (prop.PropertyType == typeof(decimal?))
                    diff = (aVal is decimal dca && bVal is decimal dcb) ? dca - dcb : null;
                else
                    throw new InvalidOperationException($"Cannot process data type {prop.PropertyType}");

                prop.SetValue(result, diff);
            }
            else
            {
                prop.SetValue(result, aVal);
            }
        }

        return result;
    }
}