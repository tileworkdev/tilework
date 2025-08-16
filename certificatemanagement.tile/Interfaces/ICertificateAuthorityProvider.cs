using System.Security.Cryptography.X509Certificates;

namespace Tilework.CertificateManagement.Interfaces;

public interface ICAProvider
{
    public Task<ICAConfiguration> Register(ICAConfiguration configuration);
    public Task<(List<X509Certificate2>, ICAConfiguration)> SignCertificateRequest(string fqdn, CertificateRequest request, ICAConfiguration configuration);
    public Task<ICAConfiguration> RevokeCertificate(X509Certificate2 certificate, ICAConfiguration configuration);
}