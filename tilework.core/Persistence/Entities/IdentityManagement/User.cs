using Microsoft.AspNetCore.Identity;


namespace Tilework.Persistence.IdentityManagement.Models;

public class User : IdentityUser<Guid>
{
    public bool Active { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? LastLoginAtUtc { get; set; }
}
