using System;

namespace Tilework.LoadBalancing.Haproxy;

[AttributeUsage(AttributeTargets.Property)]
public class StatementAttribute : Attribute
{
    public string Name { get; private set; }

    public StatementAttribute(string name)
    {
        Name = name;
    }
}