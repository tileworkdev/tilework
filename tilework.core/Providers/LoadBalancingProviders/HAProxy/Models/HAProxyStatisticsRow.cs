namespace Tilework.LoadBalancing.Haproxy;

public class HAProxyStatisticsRow
{
    // Identity
    public string pxname { get; init; } = "";
    public string svname { get; init; } = "";

    // Queue
    public int? qcur { get; init; }
    public int? qmax { get; init; }
    public int? qlimit { get; init; }

    // Sessions
    public int? scur { get; init; }
    public int? smax { get; init; }
    public int? slim { get; init; }
    public long? stot { get; init; }

    // Bytes
    public long? bin { get; init; }
    public long? bout { get; init; }

    // Denials / errors / retries
    public long? dreq { get; init; }
    public long? dresp { get; init; }
    public long? ereq { get; init; }
    public long? econ { get; init; }
    public long? eresp { get; init; }
    public long? wretr { get; init; }
    public long? wredis { get; init; }

    // HTTP responses
    public long? hrsp_1xx { get; init; }
    public long? hrsp_2xx { get; init; }
    public long? hrsp_3xx { get; init; }
    public long? hrsp_4xx { get; init; }
    public long? hrsp_5xx { get; init; }
    public long? hrsp_other { get; init; }

    // Queue/connect/response times (ms)
    public int? qtime { get; init; }
    public int? ctime { get; init; }
    public int? rtime { get; init; }
    public int? ttime { get; init; }

    // Status & health
    public string? status { get; init; }
    public int? weight { get; init; }
    public int? act { get; init; }
    public int? bck { get; init; }
    public long? chkfail { get; init; }
    public long? chkdown { get; init; }
    public int? lastchg { get; init; }
    public long? downtime { get; init; }
    public long? qlimit_id { get; init; }   // sometimes appears depending on build

    // Throttle / rate
    public int? throttle { get; init; }
    public int? lbtot { get; init; }

    // Check info
    public string? tracked { get; init; }
    public string? type { get; init; }
    public int? rate { get; init; }
    public int? rate_lim { get; init; }
    public int? rate_max { get; init; }

    // HTTP specifics
    public string? check_status { get; init; }
    public int? check_code { get; init; }
    public long? check_duration { get; init; }

    // Compression
    public long? comp_in { get; init; }
    public long? comp_out { get; init; }
    public long? comp_byp { get; init; }

    // HTTP cache
    public long? cache_hits { get; init; }
    public long? cache_miss { get; init; }

    // Last / agent info
    public string? srv_abrt { get; init; }
    public string? cli_abrt { get; init; }

    // Extra fields (newer HAProxy may add more, keep as strings)
    public string? lastsess { get; init; }
    public string? qid { get; init; }
}