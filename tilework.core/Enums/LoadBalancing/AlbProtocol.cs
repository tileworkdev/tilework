using System.ComponentModel;

namespace Tilework.Core.LoadBalancing.Enums;

public enum AlbProtocol
{
    [Description("HTTP")]
    HTTP,
    [Description("HTTPS")]
    HTTPS
}