using System.ComponentModel.DataAnnotations;

using Tilework.CertificateManagement.Enums;
using Tilework.CertificateManagement.Persistence.Models;

namespace Tilework.Ui.Models;

public class NewCertificateForm : BaseForm
{
    [Required]
    public string Name { get; set; }

    [Required]
    public string Fqdn { get; set; }

    [Required]
    public KeyAlgorithm Algorithm { get; set; }

    [Required]
    public CertificateAuthority? Authority { get; set; }
}