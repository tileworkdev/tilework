using Tilework.CertificateManagement.Services;
using Tilework.CertificateManagement.Persistence.Models;
using Tilework.Ui.Interfaces;

namespace Tilework.Ui.ViewModels;

public class CertificateAuthorityListViewModel : IListViewModel<CertificateAuthority>
{

    private readonly CertificateManagementService _certificateManagementService;

    public List<CertificateAuthority> Items { get; set; } = new List<CertificateAuthority>();

    public CertificateAuthorityListViewModel(CertificateManagementService certificateManagementService)
    {
        _certificateManagementService = certificateManagementService;
    }

    public async Task Initialize()
    {
        Items = await _certificateManagementService.GetCertificateAuthorities();
    }
}