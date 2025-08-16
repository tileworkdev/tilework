using Tilework.Core.CertificateManagement.Enums;
using Microsoft.EntityFrameworkCore;

namespace Tilework.CertificateManagement.Persistence.Models;

[Index(nameof(Name), IsUnique = true)]
public class CertificateAuthority
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public CertificateAuthorityType Type { get; set; }
    public string Parameters { get; set; }
}