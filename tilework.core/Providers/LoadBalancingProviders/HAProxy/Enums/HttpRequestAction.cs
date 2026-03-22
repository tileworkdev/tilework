using System.ComponentModel;

namespace Tilework.LoadBalancing.Haproxy;

public enum HttpRequestAction
{
    Redirect,
    Return,
    Deny,
    SetVariable
}