using Tilework.Core.Enums;

namespace Tilework.Core.Models;

public class Container
{
    public string Id { get; set; }
    public string Name { get; set; }
    public ContainerState State { get; set; }
}