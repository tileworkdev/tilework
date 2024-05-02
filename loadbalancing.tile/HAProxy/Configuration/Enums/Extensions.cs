using System;
using System.ComponentModel;
using System.Reflection;
using System.Linq;

namespace Tilework.LoadBalancing.Haproxy;

public static class EnumExtensions
{
    public static string GetDescription(this Enum enumValue)
    {
        return enumValue.GetType()
                   .GetMember(enumValue.ToString())
                   .First()
                   .GetCustomAttribute<DescriptionAttribute>()?
                   .Description ?? string.Empty;
    }
}