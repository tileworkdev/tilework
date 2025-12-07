namespace Tilework.Ui.Services;

public interface IBrowserTimeZoneProvider
{
    ValueTask<TimeZoneInfo> GetTimeZoneAsync(CancellationToken cancellationToken = default);
}
