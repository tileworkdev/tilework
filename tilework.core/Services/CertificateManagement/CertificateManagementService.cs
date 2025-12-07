using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

using AutoMapper;
using Coravel.Events.Interfaces;

using Tilework.Events;
using Tilework.Persistence.CertificateManagement.Models;
using Tilework.CertificateManagement.Models;
using Tilework.CertificateManagement.Interfaces;
using Tilework.CertificateManagement.Enums;
using Tilework.Core.Persistence;

namespace Tilework.CertificateManagement.Services;

public class CertificateManagementService : ICertificateManagementService
{
    private readonly TileworkContext _dbContext;
    private readonly CertificateManagementConfiguration _settings;
    private readonly ILogger<CertificateManagementService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMapper _mapper;
    private readonly IDispatcher _dispatcher;

    public CertificateManagementService(TileworkContext dbContext,
                                        IDispatcher dispatcher,
                                        IMapper mapper,
                                        IOptions<CertificateManagementConfiguration> settings,
                                        ILogger<CertificateManagementService> logger,
                                        IServiceProvider serviceProvider)
    {
        _dbContext = dbContext;
        _logger = logger;
        _settings = settings.Value;
        _serviceProvider = serviceProvider;
        _dispatcher = dispatcher;
        _mapper = mapper;
    }

    private (ICAProvider, ICAConfiguration) GetProvider(CertificateAuthority certificateAuthority)
    {
        return certificateAuthority.Type switch
        {
            CertificateAuthorityType.ACME => (
                _serviceProvider.GetRequiredService<AcmeProvider>(),
                JsonSerializer.Deserialize<AcmeConfiguration>(certificateAuthority.Parameters)!
                ),
            CertificateAuthorityType.LETSENCRYPT => (
                _serviceProvider.GetRequiredService<AcmeProvider>(),
                JsonSerializer.Deserialize<LetsEncryptConfiguration>(certificateAuthority.Parameters)!
                ),
            _ => throw new ArgumentException($"Invalid CA provider {certificateAuthority.Type}")
        };
    }

    private string DeserializeConfig(ICAConfiguration config)
    {
        return config switch
        {
            AcmeConfiguration acme => JsonSerializer.Serialize(acme),
            _ => throw new NotSupportedException(config.GetType().Name)
        };
    }

    private bool TryMarkCertificateExpired(Certificate? certificate)
    {
        if (certificate == null)
            return false;

        var expiresAt = certificate.ExpiresAtUtc;

        if (!expiresAt.HasValue || certificate.Status == CertificateStatus.EXPIRED)
            return false;

        if (expiresAt.Value > DateTimeOffset.UtcNow)
            return false;

        certificate.Status = CertificateStatus.EXPIRED;
        return true;
    }


    public async Task<List<CertificateDTO>> GetCertificates()
    {
        var entities = await _dbContext.Certificates.ToListAsync();
        var changed = false;

        foreach (var certificate in entities)
        {
            changed |= TryMarkCertificateExpired(certificate);
        }

        if (changed)
        {
            await _dbContext.SaveChangesAsync();
        }

        return _mapper.Map<List<CertificateDTO>>(entities);
    }

    public async Task<CertificateDTO?> GetCertificate(Guid Id)
    {
        var entity = await _dbContext.Certificates.FindAsync(Id);

        if (entity == null)
            return null;

        if (TryMarkCertificateExpired(entity))
            await _dbContext.SaveChangesAsync();

        return _mapper.Map<CertificateDTO>(entity);
    }

    public async Task<CertificateDTO> AddCertificate(string name, string fqdn, KeyAlgorithm algorithm, Guid authorityId)
    {
        var nameExists = await _dbContext.Certificates.AnyAsync(c => c.Name == name);
        if (nameExists)
            throw new ArgumentException($"Certificate with name '{name}' already exists.", nameof(name));

        var authority = await _dbContext.CertificateAuthorities.FindAsync(authorityId);
        var key = GenerateKey(algorithm);

        var certificate = new Certificate()
        {
            Name = name,
            Fqdn = fqdn,
            Authority = authority,
            PrivateKey = key
        };


        await SignCertificate(certificate);

        _dbContext.Certificates.Add(certificate);
        await _dbContext.SaveChangesAsync();

        return _mapper.Map<CertificateDTO>(certificate);
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

        return new PrivateKey()
        {
            Algorithm = algorithm,
            KeyData = keyAlg
        };
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

        (var chain, config) = await provider.SignCertificateRequest(certificate.Fqdn, csr, config);

        certificate.CertificateData = chain;
        certificate.ExpiresAtUtc = new DateTimeOffset(certificate.CertificateData.First().NotAfter.ToUniversalTime());
        certificate.Status = CertificateStatus.ACTIVE;

        certificate.Authority.Parameters = DeserializeConfig(config);
    }

    private async Task RevokeCertificate(Certificate certificate)
    {
        if (certificate.Status != CertificateStatus.ACTIVE)
            throw new InvalidOperationException($"Cannot revoke certificate: status is {certificate.Status}");
        if (certificate.CertificateData == null)
            throw new InvalidOperationException($"Cannot revoke certificate: no certificate data found");


        (var provider, var config) = GetProvider(certificate.Authority);
        config = await provider.RevokeCertificate(certificate.CertificateData.First(), config);
        certificate.Authority.Parameters = DeserializeConfig(config);
        certificate.Status = CertificateStatus.REVOKED;
        await _dbContext.SaveChangesAsync();
    }

    public async Task RevokeCertificate(Guid Id)
    {
        var certificate = await _dbContext.Certificates.FindAsync(Id);
        if(certificate == null)
            throw new ArgumentException($"Certificate {Id} not found");

        await RevokeCertificate(certificate);
    }

    private async Task<CertificateDTO> RenewCertificate(Certificate certificate)
    {
        certificate.Status = CertificateStatus.NEW;
        certificate.PrivateKey = GenerateKey(certificate.PrivateKey.Algorithm);
        await SignCertificate(certificate);
        await _dbContext.SaveChangesAsync();

        var cert = _mapper.Map<CertificateDTO>(certificate);

        var evt = new CertificateRenewed(cert);
        await _dispatcher.Broadcast(evt);

        return cert;
    }

    public async Task<CertificateDTO> RenewCertificate(Guid Id)
    {
        var certificate = await _dbContext.Certificates.FindAsync(Id);
        if(certificate == null)
            throw new ArgumentException($"Certificate {Id} not found");

        return await RenewCertificate(certificate);
    }

    public async Task RenewExpiringCertificates()
    {
        var renewalThreshold = DateTimeOffset.UtcNow.Add(_settings.CertRenewalLeadTime);

        var certs = await _dbContext.Certificates
            .Where(c => c.ExpiresAtUtc.HasValue && c.ExpiresAtUtc <= renewalThreshold)
            .ToListAsync();

        foreach(var cert in certs)
        {
            _logger.LogInformation($"Certificate [{cert.Name}] needs renewal. Renewing");
            await RenewCertificate(cert.Id);
        }
    }

    public async Task DeleteCertificate(Guid Id)
    {
        var certificate = await _dbContext.Certificates.FindAsync(Id);

        if (certificate.Status == CertificateStatus.ACTIVE)
            await RevokeCertificate(certificate);

        _dbContext.Certificates.Remove(certificate);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<PrivateKeyDTO?> GetPrivateKey(Guid Id)
    {
        var entity = await _dbContext.PrivateKeys.FindAsync(Id);
        return _mapper.Map<PrivateKeyDTO>(entity);
    }

    public async Task<List<CertificateAuthorityDTO>> GetCertificateAuthorities()
    {
        var entities = await _dbContext.CertificateAuthorities.ToListAsync();
        return _mapper.Map<List<CertificateAuthorityDTO>>(entities);
    }

    public async Task<CertificateAuthorityDTO?> GeCertificateAuthority(Guid Id)
    {
        var entity = await _dbContext.CertificateAuthorities.FindAsync(Id);
        return _mapper.Map<CertificateAuthorityDTO>(entity);
    }

    public async Task AddCertificateAuthority(CertificateAuthorityDTO authority)
    {
        var entity = _mapper.Map<CertificateAuthority>(authority);
        (var provider, var config) = GetProvider(entity);
        config = await provider.Register(config);
        entity.Parameters = DeserializeConfig(config);

        _dbContext.CertificateAuthorities.Add(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteCertificateAuthority(Guid Id)
    {
        var authority = await _dbContext.CertificateAuthorities.FindAsync(Id);
        _dbContext.CertificateAuthorities.Remove(authority);
        await _dbContext.SaveChangesAsync();
    }
}
