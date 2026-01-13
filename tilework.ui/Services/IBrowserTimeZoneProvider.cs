namespace Tilework.Ui.Services;

public interface IBrowserTimeZoneProvider
{
    Task Initialize(CancellationToken cancellationToken = default);
    ValueTask<TimeZoneInfo> GetTimeZoneAsync(CancellationToken cancellationToken = default);
    DateTimeOffset Localize(DateTimeOffset value);
    DateTimeOffset? Localize(DateTimeOffset? value);
}
