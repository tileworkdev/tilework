using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tilework.Logging.Loki;

internal sealed class QueryRangeResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public QueryRangeData? Data { get; set; }
}

internal sealed class QueryRangeData
{
    [JsonPropertyName("resultType")]
    public string ResultType { get; set; } = string.Empty;

    [JsonPropertyName("result")]
    public List<QueryRangeStream> Result { get; set; } = new();
}

internal sealed class QueryRangeStream
{
    [JsonPropertyName("stream")]
    public Dictionary<string, string> Labels { get; set; } = new();

    [JsonPropertyName("values")]
    public List<List<JsonElement>> Values { get; set; } = new();
}
