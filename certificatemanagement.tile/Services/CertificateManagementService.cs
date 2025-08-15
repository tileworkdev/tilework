using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;


using Tilework.CertificateManagement.Persistence;
using Tilework.CertificateManagement.Persistence.Models;
using Tilework.CertificateManagement.Settings;
using Tilework.CertificateManagement.Enums;
using Tilework.CertificateManagement.Interfaces;
using Tilework.CertificateManagement.Models;
using Tilework.Core.Interfaces;

namespace Tilework.CertificateManagement.Services;

public class CertificateManagementService : ICertificateManagementService
{
    private readonly CertificateManagementContext _dbContext;
    private readonly CertificateManagementSettings _settings;
    private readonly ILogger<CertificateManagementService> _logger;
    // private readonly AcmeVerificationService _verificationService;
    private readonly IServiceProvider _serviceProvider;


    public CertificateManagementService(CertificateManagementContext dbContext,
                                        IOptions<CertificateManagementSettings> settings,
                                        ILogger<CertificateManagementService> logger,
                                        IServiceProvider serviceProvider)
    {
        _dbContext = dbContext;
        _logger = logger;
        _settings = settings.Value;
        _serviceProvider = serviceProvider;
    }

    private (ICAProvider, ICAConfiguration) GetProvider(CertificateAuthority certificateAuthority)
    {
        return (
            _serviceProvider.GetRequiredService<AcmeProvider>(),
            JsonSerializer.Deserialize<AcmeConfiguration>(certificateAuthority.Parameters)!
        );
    }

    private string DeserializeConfig(ICAConfiguration config)
    {
        return config switch
        {
            AcmeConfiguration acme => JsonSerializer.Serialize(acme),
            _ => throw new NotSupportedException(config.GetType().Name)
        };
    }


    public async Task<List<Certificate>> GetCertificates()
    {
        return await _dbContext.Certificates.ToListAsync();
    }

    public async Task<Certificate?> GetCertificate(Guid Id)
    {
        return await _dbContext.Certificates.FindAsync(Id);
    }

    public async Task<Certificate> AddCertificate(string name, string fqdn, CertificateAuthority authority, KeyAlgorithm algorithm)
    {
        var key = GenerateKey(algorithm);

        var certificate = new Certificate()
        {
            Name = name,
            Fqdn = fqdn,
            Authority = authority,
            PrivateKey = key
        };

         _dbContext.Certificates.Add(certificate);

        // TODO: Currently, the process is synchronous so either everything succeeds or nothing.
        // Eventually, the signing process should be done in the background and we could save the
        // thing here
        // await _dbContext.SaveChangesAsync();

        await SignCertificate(certificate);

        return certificate;
    }

    private PrivateKey GenerateKey(KeyAlgorithm algorithm)
    {
        AsymmetricAlgorithm keyAlg = algorithm switch
        {
            KeyAlgorithm.RSA_2048 => RSA.Create(2048),
            KeyAlgorithm.RSA_3072 => RSA.Create(3072),
            KeyAlgorithm.RSA_4096 => RSA.Create(4096),
            KeyAlgorithm.ECDSA_P256 => ECDsa.Create(ECCurve.NamedCurves.nistP256),
            KeyAlgorithm.ECDSA_P384 => ECDsa.Create(ECCurve.NamedCurves.nistP384),
            _ => throw new NotImplementedException(),
        };

        return new PrivateKey() { KeyData = keyAlg };        
    }

    private CertificateRequest GenerateCsr(Certificate certificate)
    {
        CertificateRequest csr;
        if(certificate.PrivateKey.KeyData is RSA rsaKey)
        {
            csr = new CertificateRequest(
                $"CN={certificate.Fqdn}",
                rsaKey,
                HashAlgorithmName.SHA256, 
                RSASignaturePadding.Pkcs1
            );
        }
        else if(certificate.PrivateKey.KeyData is ECDsa ecKey)
        {
            csr = new CertificateRequest(
                $"CN={certificate.Fqdn}",
                ecKey,
                HashAlgorithmName.SHA256
            );
        }
        else
            throw new ArgumentException("Invalid private key type for CSR generation");

        csr.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(false, false, 0, false));

        csr.CertificateExtensions.Add(
            new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, true));

        return csr;
    }

    private async Task SignCertificate(Certificate certificate)
    {
        if (certificate.Status != CertificateStatus.NEW)
            throw new InvalidOperationException($"Cannot issue certificate: status is {certificate.Status}");

        (var provider, var config) = GetProvider(certificate.Authority);

        var csr = GenerateCsr(certificate);

        (var crt, config) = await provider.SignCertificateRequest(certificate.Fqdn, csr, config);

        certificate.CertificateData = crt;
        certificate.ExpiresAtUtc = new DateTimeOffset(crt.NotAfter.ToUniversalTime());
        certificate.Status = CertificateStatus.ACTIVE;

        certificate.Authority.Parameters = DeserializeConfig(config);

        await _dbContext.SaveChangesAsync();
    }

    public async Task RevokeCertificate(Certificate certificate)
    {
        if (certificate.Status != CertificateStatus.ACTIVE)
            throw new InvalidOperationException($"Cannot revoke certificate: status is {certificate.Status}");
        if (certificate.CertificateData == null)
            throw new InvalidOperationException($"Cannot revoke certificate: no certificate data found");


        (var provider, var config) = GetProvider(certificate.Authority);
        config = await provider.RevokeCertificate(certificate.CertificateData!, config);
        certificate.Authority.Parameters = DeserializeConfig(config);
        certificate.Status = CertificateStatus.REVOKED;
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteCertificate(Certificate certificate)
    {
        if(certificate.Status == CertificateStatus.ACTIVE)
            await RevokeCertificate(certificate);

        _dbContext.Certificates.Remove(certificate);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<CertificateAuthority>> GetCertificateAuthorities()
    {
        return await _dbContext.CertificateAuthorities.ToListAsync();
    }

    public async Task<CertificateAuthority?> GeCertificateAuthority(Guid Id)
    {
        return await _dbContext.CertificateAuthorities.FindAsync(Id);
    }

    public async Task AddCertificateAuthority(CertificateAuthority authority)
    {
        (var provider, var config) = GetProvider(authority);
        config = await provider.Register(config);
        authority.Parameters = DeserializeConfig(config);

        _dbContext.CertificateAuthorities.Add(authority);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteCertificateAuthority(CertificateAuthority authority)
    {
        _dbContext.CertificateAuthorities.Remove(authority);
        await _dbContext.SaveChangesAsync();
    }
}