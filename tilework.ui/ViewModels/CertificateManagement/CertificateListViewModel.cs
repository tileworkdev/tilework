using Tilework.CertificateManagement.Services;
using Tilework.CertificateManagement.Persistence.Models;

namespace Tilework.Ui.ViewModels;

public class CertificateListViewModel
{

    private readonly CertificateManagementService _certificateManagementService;

    public List<Certificate> Certificates { get; set; } = new List<Certificate>();

    public CertificateListViewModel(CertificateManagementService certificateManagementService)
    {
        _certificateManagementService = certificateManagementService;
    }

    public async Task Initialize()
    {
        Certificates = await _certificateManagementService.GetCertificates();
    }
}