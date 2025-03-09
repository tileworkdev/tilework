using Tilework.CertificateManagement.Services;
using Tilework.CertificateManagement.Persistence.Models;

namespace Tilework.Ui.ViewModels;

public class CertificateNewViewModel
{

    private readonly CertificateManagementService _certificateManagementService;

    public Certificate Object;

    public CertificateNewViewModel(CertificateManagementService certificateManagementService)
    {
        _certificateManagementService = certificateManagementService;
    }

    public async Task Initialize()
    {
        Object = new Certificate();
    }

    public async Task Save()
    {
        // await _certificateManagementService.AddCertificate(Object);
    }
}