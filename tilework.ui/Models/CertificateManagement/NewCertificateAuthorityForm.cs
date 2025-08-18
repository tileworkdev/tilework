using System.ComponentModel.DataAnnotations;
using Tilework.CertificateManagement.Enums;

namespace Tilework.Ui.Models;

public class NewCertificateAuthorityForm : BaseForm
{
    [Required]
    public string? Name { get; set; }
    public CertificateAuthorityType Type { get; set; }
}

public class NewPredefinedAcmeCertificateAuthorityForm : NewCertificateAuthorityForm
{
    [Required, EmailAddress]
    public string? Email { get; set; }
    public bool AcceptTos { get; set; } = false;
}

public class NewAcmeCertificateAuthorityForm : NewCertificateAuthorityForm
{
    [Required]
    public string? DirectoryUrl { get; set; }
    [Required, EmailAddress]
    public string? Email { get; set; }
    public bool AcceptTos { get; set; } = false;
}
