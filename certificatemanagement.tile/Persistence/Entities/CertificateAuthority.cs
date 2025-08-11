namespace Tilework.CertificateManagement.Persistence.Models;


public class CertificateAuthority
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string DirectoryUrl { get; set; }
    public string KeyData { get; set; }
    public string Kid { get; set; }
}