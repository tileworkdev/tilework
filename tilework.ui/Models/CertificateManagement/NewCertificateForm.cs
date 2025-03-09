using Tilework.CertificateManagement.Enums;
using Tilework.CertificateManagement.Persistence.Models;

namespace Tilework.Ui.Models;

public class NewCertificateForm
{
    public string Name { get; set; }
    public string Fqdn { get; set; }

    public KeyAlgorithm Algorithm { get; set; }
    public CertificateAuthority Authority { get; set; }
}