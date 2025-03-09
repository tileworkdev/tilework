using Tilework.CertificateManagement.Services;
using Tilework.CertificateManagement.Persistence.Models;
using Tilework.Ui.Models;

namespace Tilework.Ui.ViewModels;

public class CertificateNewViewModel
{

    private readonly CertificateManagementService _certificateManagementService;

    public NewCertificateForm Object;

    public List<CertificateAuthority> Authorities;

    public CertificateNewViewModel(CertificateManagementService certificateManagementService)
    {
        _certificateManagementService = certificateManagementService;
    }

    public async Task Initialize()
    {
        Object = new NewCertificateForm();
        Authorities = await _certificateManagementService.GetCertificateAuthorities();
    }

    public async Task Save()
    {
        await _certificateManagementService.AddCertificate(Object.Name, Object.Fqdn, Object.Authority, Object.Algorithm);
    }
}