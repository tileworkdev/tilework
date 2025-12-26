using System.Text.Json;
using System.Text.RegularExpressions;
using Tilework.Core.Enums;

namespace Tilework.Exceptions.Core;

public class DockerException : Exception
{
    public ContainerExceptionType? Type { get; set; } = null;

    private static readonly Regex PortConflictRegex = new(
        @"failed\s+to\s+bind\s+host\s+port\s+[^\s:]+:\d+(?:/\w+)?:\s+address\s+already\s+in\s+use",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public DockerException(string message) : base(BuildExceptionMessage(message, out var parsedMessage))
    {
        Type = DetectExceptionType(parsedMessage);
    }

    private static ContainerExceptionType? DetectExceptionType(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return null;

        if (PortConflictRegex.IsMatch(message))
            return ContainerExceptionType.PORT_CONFLICT;

        return null;
    }

    private static string BuildExceptionMessage(string message, out string? parsedMessage)
    {
        parsedMessage = ParseResponseBody(message);
        return parsedMessage ?? message;
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
