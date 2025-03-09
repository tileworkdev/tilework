using Tilework.CertificateManagement.Services;
using Tilework.CertificateManagement.Persistence.Models;
using Tilework.Ui.Models;

namespace Tilework.Ui.ViewModels;

public class CertificateAuthorityNewViewModel
{

    private readonly CertificateManagementService _certificateManagementService;

    public CertificateAuthority Object;

    public CertificateAuthorityNewViewModel(CertificateManagementService certificateManagementService)
    {
        _certificateManagementService = certificateManagementService;
    }

    public async Task Initialize()
    {
        Object = new CertificateAuthority();
    }

    public async Task Save()
    {
        await _certificateManagementService.AddCertificateAuthority(Object);
    }
}