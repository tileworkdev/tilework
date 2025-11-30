using System;
using System.Net;
using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Logging;


namespace Tilework.Core.Services;

public class HttpApiService
{
    private readonly ILogger<HttpApiService> _logger;

    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public HttpApiService(ILogger<HttpApiService> logger, string baseUrl, TimeSpan? timeout = null)
    {
        _logger = logger;
        _httpClient = new HttpClient
        {
            Timeout = timeout ?? TimeSpan.FromSeconds(10)
        };
        _baseUrl = baseUrl;
    }

    public async Task<T> ApiGet<T>(string url,
                                   Dictionary<string, string>? headers = null,
                                   Dictionary<string, string>? query = null) where T : class
    {
        return await ApiCall<T>(HttpMethod.Get, url, headers, query);
    }
    
    public async Task<T> ApiPost<T>(string url,
                                    Dictionary<string, string>? headers = null,
                                    Dictionary<string, string>? query = null,
                                    object? requestData = null) where T : class
    {
        return await ApiCall<T>(HttpMethod.Post, url, headers, query, requestData);
    }


    private async Task<T> ApiCall<T>(HttpMethod method, string url,
                                     Dictionary<string, string>? headers = null,
                                     Dictionary<string, string>? query = null,
                                     object? requestData = null) where T : class
    {
        var requestUrl = $"{_baseUrl.TrimEnd('/')}/{url.TrimStart('/')}";

        if (query is not null && query.Count > 0)
        {
            var q = string.Join("&", query.Select(kv =>
                $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
            requestUrl = $"{requestUrl}?{q}";
        }

        using var request = new HttpRequestMessage(method, requestUrl);

        if (headers is not null)
        {
            foreach (var h in headers)
            {
                request.Headers.TryAddWithoutValidation(h.Key, h.Value);
            }
        }

        string? serializedRequestData = null;
        if(requestData != null)
        {
            serializedRequestData = JsonSerializer.Serialize(requestData);
            request.Content = new StringContent(serializedRequestData, Encoding.UTF8, "application/json");            
        }

        var headerLog = headers is not null && headers.Count > 0
            ? string.Join(", ", headers.Select(kv => $"{kv.Key}={kv.Value}"))
            : "";
        var bodyLog = serializedRequestData ?? "";

        _logger.LogDebug("Sending HTTP {Method} {Url}\nHeaders: {Headers}\nBody: {Body}",
                         method, requestUrl, headerLog, bodyLog);

        try
        {
            HttpResponseMessage response = await _httpClient.SendAsync(request);

            string responseBody = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("Received HTTP {StatusCode} response to {Method} {Url}\nBody: {Body}",
                             (int) response.StatusCode, method, requestUrl, responseBody);

            response.EnsureSuccessStatusCode();

            var deserializedResponse = JsonSerializer.Deserialize<T>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return deserializedResponse!;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "HTTP {Method} {Url} failed.", method, requestUrl);
            throw;
        }
    }
}
