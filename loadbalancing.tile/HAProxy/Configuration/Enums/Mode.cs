using System.ComponentModel;

namespace Tilework.LoadBalancing.Haproxy;

public enum Mode
{
    [Description("http")]
    HTTP,
    [Description("tcp")]
    TCP
}