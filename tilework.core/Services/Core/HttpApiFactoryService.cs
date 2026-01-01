using Microsoft.Extensions.Logging;

namespace Tilework.Core.Services;

public class HttpApiFactoryService
{
    private readonly ILoggerFactory _loggerFactory;

    public HttpApiFactoryService(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public HttpApiService GetApiService(string baseUrl)
    {
        var logger = _loggerFactory.CreateLogger<HttpApiService>();
        return new HttpApiService(logger, baseUrl);
    }
}
