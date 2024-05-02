using System.ComponentModel;

namespace Tilework.LoadBalancing.Haproxy;

public enum BalanceMode
{
    [Description("roundrobin")]
    ROUNDROBIN,
    [Description("static-rr")]
    STATIC_RR,
    [Description("leastconn")]
    LEASTCONN,
    [Description("first")]
    FIRST,
    [Description("has")]
    HASH,
    [Description("source")]
    SOURCE,
    [Description("uri")]
    URI,
    [Description("url_param")]
    URL_PARAM
}