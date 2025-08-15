using System.ComponentModel.DataAnnotations;

namespace Tilework.Ui.Models;

public class NewCertificateAuthorityForm : BaseForm
{
    [Required]
    public string Name { get; set; }
}

public class NewAcmeCertificateAuthorityForm : NewCertificateAuthorityForm
{
    [Required]
    public string DirectoryUrl { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }
}