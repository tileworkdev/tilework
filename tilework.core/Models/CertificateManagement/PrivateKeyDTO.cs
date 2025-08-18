using System.Security.Cryptography;

using Tilework.CertificateManagement.Enums;


namespace Tilework.CertificateManagement.Models;

public class PrivateKeyDTO
{
    public Guid Id { get; set; }
    public KeyAlgorithm Algorithm { get; set; }
    public AsymmetricAlgorithm KeyData { get; set; }
}