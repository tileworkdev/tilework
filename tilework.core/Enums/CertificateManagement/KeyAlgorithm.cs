using System.ComponentModel;

namespace Tilework.Core.CertificateManagement.Enums;

public enum KeyAlgorithm
{
    [Description("RSA 2048bit")]
    RSA_2048,
    [Description("RSA 3072bit")]
    RSA_3072,
    [Description("RSA 4096bit")]
    RSA_4096,
    [Description("ECDSA P-256")]
    ECDSA_P256,
    [Description("ECDSA P-384")]
    ECDSA_P384
}