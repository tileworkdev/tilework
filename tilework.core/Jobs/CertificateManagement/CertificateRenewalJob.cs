using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using Tilework.CertificateManagement.Interfaces;
using Tilework.LoadBalancing.Interfaces;

namespace Tilework.Core.Jobs.CertificateManagement;

public class CertificateRenewalJob : IInvocable
{
    private readonly ICertificateManagementService _certificateManagementService;
    private readonly ILogger<CertificateRenewalJob> _logger;
    public CertificateRenewalJob(ICertificateManagementService certificateManagementService,
                                 ILogger<CertificateRenewalJob> logger)
    {
        _certificateManagementService = certificateManagementService;
        _logger = logger;
    }

    public async Task Invoke()
    {
        _logger.LogInformation("Running scheduled certificate renewal check");
        await _certificateManagementService.RenewExpiringCertificates();
    }
}
