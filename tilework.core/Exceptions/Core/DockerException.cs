using System.Text.Json;

namespace Tilework.Exceptions.Core;

public class DockerException : Exception
{
    public DockerException(string message) : base(ParseResponseBody(message))
    {
    }

    private static string? ParseResponseBody(string message)
    {
        var responseBody = JsonSerializer.Deserialize<ResponseBody>(message);
        return responseBody?.message;
    }
}


public class ResponseBody
{
    public string message { get; set; }
}