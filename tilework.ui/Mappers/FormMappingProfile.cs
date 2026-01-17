using AutoMapper;

using Tilework.Ui.Models;
using Tilework.CertificateManagement.Models;
using Tilework.LoadBalancing.Models;
using Tilework.IdentityManagement.Models;

using System.Text.Json;

namespace Tilework.Ui.Mappers;

public class FormMappingProfile : Profile
{
    public FormMappingProfile()
    {
        // Target groups
        CreateMap<NewTargetGroupForm, TargetGroupDTO>();
        CreateMap<EditTargetGroupForm, TargetGroupDTO>();
        CreateMap<TargetGroupDTO, EditTargetGroupForm>();

        // Load balancers
        CreateMap<NewLoadBalancerForm, LoadBalancerDTO>();

        CreateMap<EditLoadBalancerForm, LoadBalancerDTO>();
        CreateMap<LoadBalancerDTO, EditLoadBalancerForm>();

        // Identity management
        CreateMap<NewUserForm, UserDTO>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Active, opt => opt.MapFrom(_ => true));
        CreateMap<UserDTO, EditUserForm>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName));
        CreateMap<EditUserForm, UserDTO>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username));

        // Certificate authorities
        CreateMap<NewPredefinedAcmeCertificateAuthorityForm, CertificateAuthorityDTO>()
            .ForMember(dest => dest.Parameters, opt => opt.MapFrom(src =>
                new LetsEncryptConfiguration()
                {
                    Email = src.Email!,
                    AcceptTos = src.AcceptTos
                }
            ));
        CreateMap<NewAcmeCertificateAuthorityForm, CertificateAuthorityDTO>()
            .ForMember(dest => dest.Parameters, opt => opt.MapFrom(src =>
                new AcmeConfiguration()
                {
                    DirectoryUrl = src.DirectoryUrl!,
                    Email = src.Email!,
                    AcceptTos = src.AcceptTos
                }
            ));

        CreateMap<NewAcmeCertificateAuthorityForm, NewPredefinedAcmeCertificateAuthorityForm>();
        CreateMap<NewPredefinedAcmeCertificateAuthorityForm, NewAcmeCertificateAuthorityForm>();
    }
}
