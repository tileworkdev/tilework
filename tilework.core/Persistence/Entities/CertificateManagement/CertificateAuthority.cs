using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using Tilework.CertificateManagement.Enums;
using Tilework.CertificateManagement.Interfaces;
using Tilework.CertificateManagement.Models;

namespace Tilework.Persistence.CertificateManagement.Models;

[Index(nameof(Name), IsUnique = true)]
public class CertificateAuthority
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public CertificateAuthorityType Type { get; set; }

    [Column("Parameters")]
    public string ParametersString { get; set; } = string.Empty;


    [NotMapped]
    public ICAConfiguration Parameters
    {
        get
        {
            var configType = GetConfigurationType();

            if (string.IsNullOrWhiteSpace(ParametersString))
                return (ICAConfiguration)Activator.CreateInstance(configType)!;

            var deserialized = JsonSerializer.Deserialize(ParametersString, configType) as ICAConfiguration;
            if (deserialized == null)
                throw new InvalidOperationException($"Failed to deserialize certificate authority configuration for type {Type}");

            return deserialized;
        }
        set
        {
            var type = GetConfigurationType();
            ParametersString = JsonSerializer.Serialize(value, type);
        }
    }

    private Type GetConfigurationType()
    {
        return Type switch
        {
            CertificateAuthorityType.ACME => typeof(AcmeConfiguration),
            CertificateAuthorityType.LETSENCRYPT => typeof(LetsEncryptConfiguration),
            _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unsupported certificate authority type")
        };
    }
}
