using System.Security.Cryptography.X509Certificates;

using Tilework.Core.CertificateManagement.Enums;


namespace Tilework.Core.CertificateManagement.Models;

public class CertificateDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    public string Fqdn { get; set; }

    public Guid Authority { get; set; }

    public Guid PrivateKey { get; set; }

    public DateTimeOffset? ExpiresAtUtc { get; set; }

    public CertificateStatus Status { get; set; }

    public List<X509Certificate2> CertificateData { get; set; } = new();
}