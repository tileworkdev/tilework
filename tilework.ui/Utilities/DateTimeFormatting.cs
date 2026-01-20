using System.Globalization;

namespace Tilework.Ui.Utilities;

public static class DateTimeFormatting
{
    public static string FormatTimestamp(DateTimeOffset value)
    {
        return value.ToString("d MMMM yyyy, HH:mm:ss (zzz)", CultureInfo.CurrentCulture);
    }

    public static string FormatTimestamp(DateTimeOffset? value)
    {
        return value is null ? string.Empty : FormatTimestamp(value.Value);
    }
}
