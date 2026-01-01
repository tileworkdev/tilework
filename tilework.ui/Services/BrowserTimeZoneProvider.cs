using Microsoft.JSInterop;

namespace Tilework.Ui.Services;

public sealed class BrowserTimeZoneProvider : IBrowserTimeZoneProvider
{
    private readonly IJSRuntime _jsRuntime;
    private TimeZoneInfo? _cachedTimeZone;

    public BrowserTimeZoneProvider(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async ValueTask<TimeZoneInfo> GetTimeZoneAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedTimeZone is not null)
        {
            return _cachedTimeZone;
        }

        var timeZoneId = await _jsRuntime.InvokeAsync<string>(
            identifier: "timeZoneInterop.getTimeZone",
            cancellationToken: cancellationToken);

        _cachedTimeZone = ResolveTimeZone(timeZoneId);
        return _cachedTimeZone;
    }

    private static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
    {
        if (!string.IsNullOrWhiteSpace(timeZoneId))
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {                
            }
        }

        // Fall back to UTC
        return TimeZoneInfo.Utc;
    }
}
