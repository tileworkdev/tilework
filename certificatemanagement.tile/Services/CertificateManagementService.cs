using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using ACMESharp.Protocol;
using ACMESharp.Authorizations;

using Tilework.LoadBalancing.Services;
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
    private readonly AcmeVerificationService _verificationService;


    public CertificateManagementService(CertificateManagementContext dbContext,
                                        IOptions<CertificateManagementSettings> settings,
                                        ILogger<CertificateManagementService> logger,
                                        AcmeVerificationService verificationService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _settings = settings.Value;
        _verificationService = verificationService;
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
        await _dbContext.SaveChangesAsync();

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
            new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, true));

        return csr;
    }

    private async Task CreateAcmeAccount(CertificateAuthority authority)
    {
        _logger.LogInformation($"Creating a new account at {authority.DirectoryUrl} with {authority.Email} for CA {authority.Name}");

        var httpClient = new HttpClient { BaseAddress = new Uri(authority.DirectoryUrl) };
        var acmeClient = new AcmeProtocolClient(httpClient, usePostAsGet: true);

        acmeClient.Directory = await acmeClient.GetDirectoryAsync();

        await acmeClient.GetNonceAsync();

        acmeClient.Account = await acmeClient.CreateAccountAsync(new List<string>() { $"mailto: {authority.Email}" }, true);

        authority.Kid = acmeClient.Account.Kid;
        authority.KeyData = acmeClient.Signer.Export();

        _logger.LogInformation($"Created account at {authority.DirectoryUrl} with {authority.Email} for CA {authority.Name} successfully");
    }

    private async Task SignCertificate(Certificate certificate)
    {
        var signer = new ACMESharp.Crypto.JOSE.Impl.ESJwsTool();
        signer.Import(certificate.Authority.KeyData);

        var account = new AccountDetails();
        account.Kid = certificate.Authority.Kid;

        var httpClient = new HttpClient { BaseAddress = new Uri(certificate.Authority.DirectoryUrl) };
        var acmeClient = new AcmeProtocolClient(httpClient, acct: account, signer: signer, usePostAsGet: true);

        _logger.LogInformation($"Creating order for certificate {certificate.Id} with {certificate.Authority.DirectoryUrl}");
        acmeClient.Directory = await acmeClient.GetDirectoryAsync();

        await acmeClient.GetNonceAsync();

        var order = await acmeClient.CreateOrderAsync(new List<string>() { certificate.Fqdn });

        var authzUrl = order.Payload.Authorizations.First();
        var authz = await acmeClient.GetAuthorizationDetailsAsync(authzUrl);
        var challenge = authz.Challenges.First(c => c.Type == "http-01");

        var challengeDetails = (Http01ChallengeValidationDetails) AuthorizationDecoder.DecodeChallengeValidation(authz, challenge.Type, signer);

        var verificationHost = new Uri(challengeDetails.HttpResourceUrl).Host;
        var verificationFile = Path.GetFileName(challengeDetails.HttpResourcePath);
        
        try
        {
            await _verificationService.StartVerification(
                certificate,
                verificationHost,
                verificationFile,
                challengeDetails.HttpResourceValue
            );

            _logger.LogInformation($"Requesting challenge validation for {certificate.Id} with {certificate.Authority.DirectoryUrl}");
            await acmeClient.AnswerChallengeAsync(challenge.Url);

            for (int i = 0; i < 20; i++)
            {
                challenge = await acmeClient.GetChallengeDetailsAsync(authzUrl);
                if (challenge.Status != "pending")
                {
                    _logger.LogInformation($"Challenge validation for {certificate.Id} finished, result: {challenge.Status}");
                    break;
                }

                await Task.Delay(1000);
            }
        }
        finally
        {
            await _verificationService.StopVerification(certificate);
        }
        


        

        // var authz = (await acmeClient.GetPendingAuthorizationsAsync(order)).First();
        // var challenge = authz.Challenges.First(c => c.Type == "http-01");

        // var challenge = await acmeClient.GetChallengeDetailsAsync(order.auth);

        // await _verificationService.StopVerification(certificate);

        // var csr = GenerateCsr(certificate);
    }

    public async Task DeleteCertificate(Certificate certificate)
    {
        // TODO: Should not be able to delete if not revoked
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
        _dbContext.CertificateAuthorities.Add(authority);
        await CreateAcmeAccount(authority);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteCertificateAuthority(CertificateAuthority authority)
    {
        _dbContext.CertificateAuthorities.Remove(authority);
        await _dbContext.SaveChangesAsync();
    }
}