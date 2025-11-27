using Microsoft.EntityFrameworkCore;


namespace Tilework.Persistence.TokenVault.Models;

[Index(nameof(Key), IsUnique = true)]
public class Token
{
    public Guid Id { get; set; }

    public string Key { get; set; } = default!;
    public string Value { get; set; } = default!;
}