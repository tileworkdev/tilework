using System.ComponentModel;

namespace Tilework.CertificateManagement.Enums;

public enum CertificateAuthorityType
{
    [Description("ACME")]
    ACME,
    [Description("Let's encrypt")]
    LETSENCRYPT
}