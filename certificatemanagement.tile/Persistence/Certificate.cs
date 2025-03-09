namespace Tilework.CertificateManagement.Persistence.Models;


public class Certificate
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    
    public string Fqdn { get; set; }

    public Guid AuthorityId { get; set; }
    public virtual CertificateAuthority Authority { get; set; }

    public Guid PrivateKeyId { get; set; }
    public virtual PrivateKey PrivateKey { get; set; }

    public string CertificateData { get; set; }
}