namespace Tilework.CertificateManagement.Models;

public class CertificateManagementConfiguration
{
    public string AcmeVerificationImage { get; set; }
    public TimeSpan CertRenewalLeadTime { get; set; } 
}