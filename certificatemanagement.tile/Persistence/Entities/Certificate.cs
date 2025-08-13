using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using Tilework.CertificateManagement.Enums;

namespace Tilework.CertificateManagement.Persistence.Models;


public class Certificate
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    
    public string Fqdn { get; set; }

    public Guid AuthorityId { get; set; }
    public virtual CertificateAuthority Authority { get; set; }

    public Guid PrivateKeyId { get; set; }
    public virtual PrivateKey PrivateKey { get; set; }

    public string? CertificateDataString { get; set; }

    [NotMapped]
    public CertificateStatus Status
    {
        get {
            if(CertificateData == null)
                return CertificateStatus.NEW;
            else if(CertificateData.NotAfter > DateTime.Now)
                return CertificateStatus.EXPIRED;
            else
                return CertificateStatus.ISSUED;
        }
    }


    [NotMapped]
    public X509Certificate2? CertificateData
    {
        get {
            return string.IsNullOrEmpty(CertificateDataString) ? null : X509CertificateLoader.LoadCertificate(Encoding.ASCII.GetBytes(CertificateDataString));
        }
        set {
            if(value != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("-----BEGIN CERTIFICATE-----");
                sb.AppendLine(Convert.ToBase64String(value.RawData, Base64FormattingOptions.InsertLineBreaks));
                sb.AppendLine("-----END CERTIFICATE-----");
                CertificateDataString = sb.ToString();
            }
            else
                CertificateDataString = null;

        }
    }
}