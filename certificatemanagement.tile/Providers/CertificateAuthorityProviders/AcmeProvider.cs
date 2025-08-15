using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
// using System.Text.Json;

using ACMESharp.Protocol;
using ACMESharp.Authorizations;

using Tilework.CertificateManagement.Interfaces;
using Tilework.CertificateManagement.Models;

namespace Tilework.CertificateManagement.Services;

public class AcmeProvider : ICAProvider
{
    private readonly ILogger<AcmeProvider> _logger;
    private readonly AcmeVerificationService _verificationService;

    public AcmeProvider(ILogger<AcmeProvider> logger,
                        AcmeVerificationService verificationService)
    {
        _logger = logger;
        _verificationService = verificationService;
    }

    private async Task<AcmeConfiguration> CreateAccount(AcmeConfiguration config)
    {
        _logger.LogInformation($"Creating a new account at {config.DirectoryUrl} with {config.Email}");

        var httpClient = new HttpClient { BaseAddress = new Uri(config.DirectoryUrl) };
        var acmeClient = new AcmeProtocolClient(httpClient, usePostAsGet: true);

        acmeClient.Directory = await acmeClient.GetDirectoryAsync();

        await acmeClient.GetNonceAsync();

        acmeClient.Account = await acmeClient.CreateAccountAsync(new List<string>() { $"mailto: {config.Email}" }, true);

        config.Kid = acmeClient.Account.Kid;
        config.KeyData = acmeClient.Signer.Export();

        _logger.LogInformation($"Created account at {config.DirectoryUrl} with {config.Email} successfully");

        return config;
    }

    public async Task<ICAConfiguration> Register(ICAConfiguration configuration)
    {
        var acmeConfig = configuration as AcmeConfiguration
                 ?? throw new ArgumentException("Expected AcmeConfiguration", nameof(configuration));

        acmeConfig = await CreateAccount(acmeConfig);

        return acmeConfig;
    }


    public async Task<(X509Certificate2, ICAConfiguration)> SignCertificateRequest(string fqdn, CertificateRequest request, ICAConfiguration configuration)
    {
        var acmeConfig = configuration as AcmeConfiguration
                 ?? throw new ArgumentException("Expected AcmeConfiguration", nameof(configuration));


        if (string.IsNullOrEmpty(acmeConfig.Kid))
            acmeConfig = await CreateAccount(acmeConfig);


        var signer = new ACMESharp.Crypto.JOSE.Impl.ESJwsTool();
        signer.Import(acmeConfig.KeyData);

        var account = new AccountDetails();
        account.Kid = acmeConfig.Kid;

        var httpClient = new HttpClient { BaseAddress = new Uri(acmeConfig.DirectoryUrl) };
        var acmeClient = new AcmeProtocolClient(httpClient, acct: account, signer: signer, usePostAsGet: true);

        _logger.LogInformation($"Creating order for certificate {fqdn} with {acmeConfig.DirectoryUrl}");
        acmeClient.Directory = await acmeClient.GetDirectoryAsync();

        await acmeClient.GetNonceAsync();

        var order = await acmeClient.CreateOrderAsync(new List<string>() { fqdn });

        var authzUrl = order.Payload.Authorizations.First();
        var authz = await acmeClient.GetAuthorizationDetailsAsync(authzUrl);
        var challenge = authz.Challenges.First(c => c.Type == "http-01");

        var challengeDetails = (Http01ChallengeValidationDetails)AuthorizationDecoder.DecodeChallengeValidation(authz, challenge.Type, signer);

        var verificationHost = new Uri(challengeDetails.HttpResourceUrl).Host;
        var verificationFile = Path.GetFileName(challengeDetails.HttpResourcePath);

        var verificationId = Guid.NewGuid();

        try
        {
            await _verificationService.StartVerification(
                verificationId.ToString(),
                verificationHost,
                verificationFile,
                challengeDetails.HttpResourceValue
            );

            _logger.LogInformation($"Requesting challenge validation for {fqdn} with {acmeConfig.DirectoryUrl}");
            await acmeClient.AnswerChallengeAsync(challenge.Url);

            for (int i = 0; i < 20; i++)
            {
                challenge = await acmeClient.GetChallengeDetailsAsync(authzUrl);
                if (challenge.Status != "pending")
                {
                    _logger.LogInformation($"Challenge validation for {fqdn} finished, result: {challenge.Status}");
                    break;
                }

                await Task.Delay(1000);
            }
        }
        finally
        {
            await _verificationService.StopVerification(verificationId.ToString());
        }

        if (challenge.Status != "valid")
        {
            throw new Exception($"ACME authentication failed: {challenge.Status}");
        }


        _logger.LogInformation($"Requesting order finalization for {fqdn} with {acmeConfig.DirectoryUrl}");
        await acmeClient.FinalizeOrderAsync(order.Payload.Finalize, request.CreateSigningRequest());

        for (int i = 0; i < 20; i++)
        {
            order = await acmeClient.GetOrderDetailsAsync(order.OrderUrl);
            if (order.Payload.Status == "invalid" || order.Payload.Status == "valid")
            {
                _logger.LogInformation($"Order for {fqdn} finished, result: {order.Payload.Status}");
                break;
            }

            await Task.Delay(1000);
        }

        if (order.Payload.Status != "valid")
        {
            throw new Exception($"ACME certificate issuing failed: {order.Payload.Status}");
        }

        var cert = await acmeClient.GetOrderCertificateAsync(order);


        return (X509CertificateLoader.LoadCertificate(cert), acmeConfig);
    }


    public Task<ICAConfiguration> RevokeCertificate(X509Certificate2 certificate, ICAConfiguration configuration)
    {
        throw new NotImplementedException();
    }
}