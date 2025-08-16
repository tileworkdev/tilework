using Tilework.CertificateManagement.Interfaces;

namespace Tilework.Core.CertificateManagement.Models;

public class AcmeConfiguration : ICAConfiguration
{
    public string DirectoryUrl { get; set; }
    public bool AcceptTos { get; set; }
    public string Email { get; set; }
    public string Kid { get; set; }
    public string KeyData { get; set; }
}