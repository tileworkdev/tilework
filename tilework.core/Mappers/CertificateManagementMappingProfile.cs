using AutoMapper;

using Tilework.Persistence.CertificateManagement.Models;
using Tilework.Core.Models;
using Tilework.CertificateManagement.Models;

namespace Tilework.CertificateManagement.Mappers;

public class CertificateManagementMappingProfile : Profile
{
    public CertificateManagementMappingProfile()
    {
        CreateMap<CertificateAuthorityDTO, CertificateAuthority>()
            .ForMember(dest => dest.ParametersString, opt => opt.Ignore());
        CreateMap<CertificateAuthority, CertificateAuthorityDTO>();

        CreateMap<CertificateDTO, Certificate>()
            .ForMember(dest => dest.AuthorityId, opt => opt.MapFrom(src => src.Authority))
            .ForMember(dest => dest.PrivateKeyId, opt => opt.MapFrom(src => src.PrivateKey))
            .ForMember(dest => dest.Authority, opt => opt.Ignore())
            .ForMember(dest => dest.PrivateKey, opt => opt.Ignore());
        CreateMap<Certificate, CertificateDTO>()
            .ForMember(dest => dest.Authority, opt => opt.MapFrom(src => src.AuthorityId))
            .ForMember(dest => dest.PrivateKey, opt => opt.MapFrom(src => src.PrivateKeyId));

        CreateMap<PrivateKey, PrivateKeyDTO>();
    }
}
