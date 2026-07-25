using System.ComponentModel;

namespace Tilework.LoadBalancing.Haproxy;

public enum HttpRequestAction
{
    AddHeader,
    Redirect,
    Return,
    Deny,
    SetVariable
}
