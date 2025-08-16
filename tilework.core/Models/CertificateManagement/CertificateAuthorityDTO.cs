using Tilework.Core.CertificateManagement.Enums;

namespace Tilework.Core.CertificateManagement.Models;

public class CertificateAuthorityDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public CertificateAuthorityType Type { get; set; }
    public string Parameters { get; set; }
}