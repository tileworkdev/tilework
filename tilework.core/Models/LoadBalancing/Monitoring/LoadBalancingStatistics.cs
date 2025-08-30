namespace Tilework.LoadBalancing.Models;

public class LoadBalancingStatistics
{
    public TimeSpan Uptime { get; init; }
    // public int? CurrentSessions { get; init; }
    // public int? CurrentQueue { get; init; }



    // // Queue

    // public int? MaxQueue { get; init; }
    // public int? QueueLimit { get; init; }

    // // Sessions / connections

    // public int? MaxSessions { get; init; }
    // public int? SessionLimit { get; init; }
    public long? TotalSessions { get; init; }

    // // Traffic
    // public long? BytesIn { get; init; }
    // public long? BytesOut { get; init; }

    // // Errors / retries
    // public long? DeniedRequests { get; init; }
    // public long? DeniedResponses { get; init; }
    // public long? RequestErrors { get; init; }
    // public long? ConnectionErrors { get; init; }
    // public long? ResponseErrors { get; init; }
    // public long? Retries { get; init; }
    // public long? Redispatches { get; init; }

    // // HTTP responses
    // public long? Responses1xx { get; init; }
    // public long? Responses2xx { get; init; }
    // public long? Responses3xx { get; init; }
    // public long? Responses4xx { get; init; }
    // public long? Responses5xx { get; init; }
    // public long? ResponsesOther { get; init; }

    // // Timings
    // public int? AvgQueueTimeMs { get; init; }
    // public int? AvgConnectTimeMs { get; init; }
    // public int? AvgResponseTimeMs { get; init; }
    // public int? AvgTotalTimeMs { get; init; }

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
}