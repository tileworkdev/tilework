using System;

using Tilework.LoadBalancing.Enums;

namespace Tilework.Core.Mappers;

public static class EnumMappers
{
    public static LoadBalancerStatus FromHaproxyStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return LoadBalancerStatus.DOWN;
        }

        var normalized = status.Trim();
        if (normalized.StartsWith("UP", StringComparison.OrdinalIgnoreCase) ||
            normalized.StartsWith("OPEN", StringComparison.OrdinalIgnoreCase))
        {
            return LoadBalancerStatus.UP;
        }

        return LoadBalancerStatus.DOWN;
    }
}
