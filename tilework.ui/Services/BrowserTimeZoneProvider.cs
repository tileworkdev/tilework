using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;

namespace Tilework.Ui.Services;

public sealed class BrowserTimeZoneProvider : IBrowserTimeZoneProvider
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<BrowserTimeZoneProvider> _logger;
    private TimeZoneInfo? _cachedTimeZone;

    public BrowserTimeZoneProvider(IJSRuntime jsRuntime,
                                   ILogger<BrowserTimeZoneProvider> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public async Task Initialize(CancellationToken cancellationToken = default)
    {
        var timeZoneId = await _jsRuntime.InvokeAsync<string>(
            identifier: "timeZoneInterop.getTimeZone",
            cancellationToken: cancellationToken);

        _cachedTimeZone = ResolveTimeZone(timeZoneId);
    }

    public async ValueTask<TimeZoneInfo> GetTimeZoneAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedTimeZone == null)
        {
            await Initialize();
        }

        return _cachedTimeZone;
    }

    public DateTimeOffset Localize(DateTimeOffset value)
    {
        if (_cachedTimeZone is null)
        {
            _logger.LogWarning("Browser timezone not initialized yet. Falling back to ToLocalTime");
            return value.ToLocalTime();
        }

        return TimeZoneInfo.ConvertTime(value, _cachedTimeZone);
    }

    public DateTimeOffset? Localize(DateTimeOffset? value)
    {
        if (value is null)
        {
            return null;
        }

        return Localize(value.Value);
    }

    private TimeZoneInfo ResolveTimeZone(string? timeZoneId)
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

        _logger.LogWarning("Could not determine local browser time. Defaulting to UTC");
        return TimeZoneInfo.Utc;
    }
}
