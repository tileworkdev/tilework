using Tilework.CertificateManagement.Services;
using Tilework.CertificateManagement.Persistence.Models;

namespace Tilework.Ui.ViewModels;

public class CertificateAuthorityListViewModel
{

    private readonly CertificateManagementService _certificateManagementService;

    public List<CertificateAuthority> Objects { get; set; } = new List<CertificateAuthority>();

    public CertificateAuthorityListViewModel(CertificateManagementService certificateManagementService)
    {
        _certificateManagementService = certificateManagementService;
    }

    public async Task Initialize()
    {
        Objects = await _certificateManagementService.GetCertificateAuthorities();
    }
}