using System.Security.Cryptography;

using Tilework.Core.CertificateManagement.Enums;


namespace Tilework.Core.CertificateManagement.Models;

public class PrivateKeyDTO
{
    public Guid Id { get; set; }
    public KeyAlgorithm Algorithm { get; set; }
    public AsymmetricAlgorithm KeyData { get; set; }
}