using Tilework.CertificateManagement.Services;
using Tilework.CertificateManagement.Persistence.Models;
using Tilework.Ui.Interfaces;

namespace Tilework.Ui.ViewModels;

public class CertificateListViewModel : IListViewModel<Certificate>
{

    private readonly CertificateManagementService _certificateManagementService;

    public List<Certificate> Items { get; set; } = new List<Certificate>();

    public CertificateListViewModel(CertificateManagementService certificateManagementService)
    {
        _certificateManagementService = certificateManagementService;
    }

    public async Task Initialize()
    {
        Items = await _certificateManagementService.GetCertificates();
    }
}