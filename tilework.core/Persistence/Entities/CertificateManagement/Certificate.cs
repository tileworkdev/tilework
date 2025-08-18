using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using Tilework.CertificateManagement.Enums;
using Tilework.Core.Utils;

namespace Tilework.Persistence.CertificateManagement.Models;

[Index(nameof(Name), IsUnique = true)]
public class Certificate
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    
    public string Fqdn { get; set; }

    public Guid AuthorityId { get; set; }
    public virtual CertificateAuthority Authority { get; set; }

    public Guid PrivateKeyId { get; set; }
    public virtual PrivateKey PrivateKey { get; set; }

    public DateTimeOffset? ExpiresAtUtc { get; set; }

    public string? CertificateDataString { get; set; }

    public CertificateStatus Status { get; set; } = CertificateStatus.NEW;


    [NotMapped]
    public List<X509Certificate2> CertificateData
    {
        get {
            return string.IsNullOrEmpty(CertificateDataString) ? new List<X509Certificate2>() : CertificateUtils.LoadPemChain(CertificateDataString);
        }
        set {
            if (value != null)
            {
                StringBuilder chain = new StringBuilder();
                foreach (var cert in value)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("-----BEGIN CERTIFICATE-----");
                    sb.AppendLine(Convert.ToBase64String(cert.RawData, Base64FormattingOptions.InsertLineBreaks));
                    sb.AppendLine("-----END CERTIFICATE-----");
                    chain.Append(sb);
                }
                
                CertificateDataString = chain.ToString();
            }
            else
                CertificateDataString = null;

        }
    }
}