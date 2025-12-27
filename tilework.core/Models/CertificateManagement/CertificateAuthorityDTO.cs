using Tilework.CertificateManagement.Enums;
using Tilework.CertificateManagement.Interfaces;

namespace Tilework.CertificateManagement.Models;

public class CertificateAuthorityDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public CertificateAuthorityType Type { get; set; }
    public ICAConfiguration Parameters { get; set; }
}