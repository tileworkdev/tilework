namespace Tilework.Models;

public class ActionItem
{
    public string Name { get; set; }
    public string? Href { get; set; }
    public Func<Task>? OnClick { get; set; }
}