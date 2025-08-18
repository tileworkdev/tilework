namespace Tilework.CertificateManagement.Interfaces;

using Tilework.CertificateManagement.Enums;
using Tilework.CertificateManagement.Models;

public interface ICertificateManagementService
{
    public Task<List<CertificateAuthorityDTO>> GetCertificateAuthorities();
    public Task<CertificateAuthorityDTO?> GeCertificateAuthority(Guid Id);
    public Task AddCertificateAuthority(CertificateAuthorityDTO authority);
    public Task DeleteCertificateAuthority(Guid Id);


    public Task<List<CertificateDTO>> GetCertificates();
    public Task<CertificateDTO?> GetCertificate(Guid Id);
    public Task<CertificateDTO> AddCertificate(string name, string fqdn, KeyAlgorithm algorithm, Guid authorityId);
    public Task RevokeCertificate(Guid Id);
    public Task DeleteCertificate(Guid Id);

    public Task<PrivateKeyDTO?> GetPrivateKey(Guid Id);
}