using Microsoft.AspNetCore.Identity;


namespace Tilework.Persistence.IdentityManagement.Models;

public class User : IdentityUser<Guid>
{
    public bool Active { get; set; }
}