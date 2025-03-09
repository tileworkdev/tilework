using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

using Tilework.CertificateManagement.Persistence;
using Tilework.CertificateManagement.Persistence.Models;
using Tilework.CertificateManagement.Settings;
using Tilework.CertificateManagement.Enums;

namespace Tilework.CertificateManagement.Services;

public class CertificateManagementService
{
    private readonly CertificateManagementContext _dbContext;
    private readonly CertificateManagementSettings _settings;
    private readonly ILogger<CertificateManagementService> _logger;
    

    public CertificateManagementService(IServiceProvider serviceProvider,
                               CertificateManagementContext dbContext,
                               IOptions<CertificateManagementSettings> settings,
                               ILogger<CertificateManagementService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _settings = settings.Value;
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
        var certificate = new Certificate()
        {
            Name = name,
            Fqdn = fqdn,
            Authority = authority,
            PrivateKey = GenerateKey(algorithm)
        };

        await _dbContext.SaveChangesAsync();

        return certificate;
    }

    private PrivateKey GenerateKey(KeyAlgorithm algorithm)
    {
        AsymmetricAlgorithm algo;
        

        switch(algorithm)
        {
            case KeyAlgorithm.RSA_2048:
                algo = RSA.Create(2048);
                break;
            case KeyAlgorithm.RSA_3072:
                algo = RSA.Create(3072);
                break;
            case KeyAlgorithm.RSA_4096:
                algo = RSA.Create(4096);
                break;
            case KeyAlgorithm.ECDSA_P256:
                algo = ECDsa.Create(ECCurve.NamedCurves.nistP256);
                break;
            case KeyAlgorithm.ECDSA_P384:
                algo = ECDsa.Create(ECCurve.NamedCurves.nistP384);
                break;
            default:
                throw new NotImplementedException();
        }

        var keyData = algo.ExportPkcs8PrivateKey();

        var pem =  "-----BEGIN PRIVATE KEY-----\n" +
                   Convert.ToBase64String(keyData, Base64FormattingOptions.InsertLineBreaks) +
                   "\n-----END PRIVATE KEY-----";

        return new PrivateKey() { KeyData = pem };
    }

    // SignCertificate()
    // {

    // }

    // DeleteCertificate()
    // {

    // }

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
        _dbContext.CertificateAuthorities.Add(authority);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteCertificateAuthority(CertificateAuthority authority)
    {
        _dbContext.CertificateAuthorities.Remove(authority);
        await _dbContext.SaveChangesAsync();
    }
}