namespace Tilework.Core.Models;

public class ContainerCommandResult
{
    public int ExitCode { get; set; }
    public string Stdout { get; set; }
    public string Stderr { get; set; }
}