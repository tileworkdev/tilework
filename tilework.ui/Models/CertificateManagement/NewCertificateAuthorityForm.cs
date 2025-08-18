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

    [Display(Name = "I accept CA terms of service")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the terms of service to continue.")]
    public bool AcceptTos { get; set; } = false;
}

public class NewAcmeCertificateAuthorityForm : NewCertificateAuthorityForm
{
    [Required]
    [Display(Name = "Directory URL")]
    public string? DirectoryUrl { get; set; }
    [Required, EmailAddress]
    public string? Email { get; set; }

    [Display(Name = "I accept CA terms of service")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the terms of service to continue.")]
    public bool AcceptTos { get; set; } = false;
}
