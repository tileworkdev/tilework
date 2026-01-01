using System.ComponentModel.DataAnnotations;

using Tilework.CertificateManagement.Enums;
using Tilework.Core.Utils;
using Tilework.Ui.Components.Validators;

namespace Tilework.Ui.Models;

public class NewCertificateForm : BaseForm
{
    [Required]
    public string Name { get; set; }

    private string? _fqdn;

    [Required]
    [Hostname]
    public string? Fqdn
    {
        get => _fqdn;
        set => _fqdn = value != null ? HostnameUtils.NormalizeHost(value) : null;
    }

    [Required]
    public KeyAlgorithm Algorithm { get; set; }

    [Required]
    public Guid? Authority { get; set; }
}
