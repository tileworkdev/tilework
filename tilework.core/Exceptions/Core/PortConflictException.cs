using System.Text.Json;
using System.Text.RegularExpressions;
using Tilework.Core.Enums;

namespace Tilework.Exceptions.Core;

public class PortConfictException : Exception
{
    public PortConfictException(string? message = null) : base(message)
    {
    }
}
