using Microsoft.JSInterop;

namespace Tilework.Ui.Services;

public sealed class DownloadService
{
    private readonly IJSRuntime _jsRuntime;

    public DownloadService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task DownloadAsync(string fileName, string contentType, Stream stream, bool leaveOpen = false)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Filename is required.", nameof(fileName));

        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Content type is required.", nameof(contentType));

        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        if (stream.CanSeek)
            stream.Position = 0;

        await using var streamRef = new DotNetStreamReference(stream);
        await _jsRuntime.InvokeVoidAsync("downloadFileFromStream", fileName, contentType, streamRef);

        if (!leaveOpen)
            await stream.DisposeAsync();
    }

    public async Task DownloadAsync(string fileName, string contentType, byte[] data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        await using var stream = new MemoryStream(data, writable: false);
        await DownloadAsync(fileName, contentType, stream);
    }
}
