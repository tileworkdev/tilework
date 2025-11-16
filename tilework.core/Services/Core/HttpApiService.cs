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

    public HttpApiService(ILogger<HttpApiService> logger, string baseUrl)
    {
        _logger = logger;
        _httpClient = new HttpClient();
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
        var fullUrl = $"{_baseUrl.TrimEnd('/')}/{url.TrimStart('/')}";

        using var request = new HttpRequestMessage(method, fullUrl);

        if (query is not null && query.Count > 0)
        {
            var q = string.Join("&", query.Select(kv =>
                $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
            fullUrl = $"{fullUrl}?{q}";
        }

        if (headers is not null)
        {
            foreach (var h in headers)
            {
                request.Headers.TryAddWithoutValidation(h.Key, h.Value);
            }
        }

        if(requestData != null)
        {
            var jsonData = JsonSerializer.Serialize(requestData);
            request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");            
        }

        HttpResponseMessage response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        string responseBody = await response.Content.ReadAsStringAsync();

        var deserializedResponse = JsonSerializer.Deserialize<T>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return deserializedResponse!;
    }
}