using System.Text.Json;
using Tilework.CertificateManagement.Enums;

namespace Tilework.CertificateManagement.Persistence.Models;


public class CertificateAuthority
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public CertificateAuthorityType Type { get; set; }
    public string Parameters { get; set; }
}