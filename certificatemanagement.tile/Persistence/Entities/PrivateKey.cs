using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;

using Tilework.CertificateManagement.Enums;

namespace Tilework.CertificateManagement.Persistence.Models;


public class PrivateKey
{
    public Guid Id { get; set; }
    public KeyAlgorithm Algorithm { get; set; }
    public string KeyDataString { get; set; }

    [NotMapped]
    public AsymmetricAlgorithm KeyData
    {
        get
        {
            var keyBytes = Convert.FromBase64String(KeyDataString);
            try
            {
                ECDsa ecdsa = ECDsa.Create();
                ecdsa.ImportPkcs8PrivateKey(keyBytes, out _);
                return ecdsa;
            }
            catch
            {
                RSA rsa = RSA.Create();
                rsa.ImportPkcs8PrivateKey(keyBytes, out _);
                return rsa;
            }
        }
        set
        {
            var keyBytes = value.ExportPkcs8PrivateKey();
            KeyDataString = Convert.ToBase64String(keyBytes);
        }
    }
}