using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

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

    // AddCertificate(string fqdn, KeyAlgorithm algorithm)
    // {

    // }

    // SignCertificate()
    // {

    // }

    // DeleteCertificate()
    // {

    // }
}