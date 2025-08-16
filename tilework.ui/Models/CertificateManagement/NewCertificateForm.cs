using System.ComponentModel.DataAnnotations;

using Tilework.Core.CertificateManagement.Enums;

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
    public Guid? Authority { get; set; }
}