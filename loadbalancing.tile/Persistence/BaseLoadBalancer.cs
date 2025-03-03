using System.ComponentModel.DataAnnotations;

public abstract class BaseLoadBalancer
{
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;
    public int Port { get; set; }

    public bool Enabled { get; set; }
}