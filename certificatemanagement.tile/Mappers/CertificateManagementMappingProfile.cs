using AutoMapper;

using Tilework.CertificateManagement.Persistence.Models;
using Tilework.Core.Models;
using Tilework.Core.CertificateManagement.Models;

namespace Tilework.CertificateManagement.Mappers;

public class CertificateManagementMappingProfile : Profile
{
    public CertificateManagementMappingProfile()
    {
        CreateMap<CertificateAuthorityDTO, CertificateAuthority>();
        CreateMap<CertificateAuthority, CertificateAuthorityDTO>();

        CreateMap<CertificateDTO, Certificate>();
        CreateMap<Certificate, CertificateDTO>();
    }
}
