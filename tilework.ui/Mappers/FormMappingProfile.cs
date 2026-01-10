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
        CreateMap<NewApplicationLoadBalancerForm, ApplicationLoadBalancerDTO>();
        CreateMap<NewNetworkLoadBalancerForm, NetworkLoadBalancerDTO>()
            .ForMember(dest => dest.TargetGroup, opt => opt.MapFrom(src => src.TargetGroup));

        CreateMap<EditApplicationLoadBalancerForm, ApplicationLoadBalancerDTO>();
        CreateMap<EditNetworkLoadBalancerForm, NetworkLoadBalancerDTO>()
            .ForMember(dest => dest.TargetGroup, opt => opt.MapFrom(src => src.TargetGroup));

        CreateMap<ApplicationLoadBalancerDTO, EditApplicationLoadBalancerForm>();
        CreateMap<NetworkLoadBalancerDTO, EditNetworkLoadBalancerForm>()
            .ForMember(dest => dest.TargetGroup, opt => opt.MapFrom(src => src.TargetGroup));

        CreateMap<NewNetworkLoadBalancerForm, NewApplicationLoadBalancerForm>();
        CreateMap<NewApplicationLoadBalancerForm, NewNetworkLoadBalancerForm>();

        CreateMap<EditNetworkLoadBalancerForm, EditApplicationLoadBalancerForm>();
        CreateMap<EditApplicationLoadBalancerForm, EditNetworkLoadBalancerForm>();

        // Identity management
        CreateMap<NewUserForm, UserDTO>()
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
