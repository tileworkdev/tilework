using AutoMapper;

using Tilework.Ui.Models;
using Tilework.CertificateManagement.Persistence.Models;
using Tilework.CertificateManagement.Models;
using Tilework.Core.LoadBalancing.Models;

using System.Text.Json;

namespace Tilework.Ui.Mappers;

public class FormMappingProfile : Profile
{
    public FormMappingProfile()
    {
        CreateMap<NewTargetGroupForm, TargetGroupDTO>();
        CreateMap<EditTargetGroupForm, TargetGroupDTO>();
        CreateMap<TargetGroupDTO, EditTargetGroupForm>();

        CreateMap<NewAcmeCertificateAuthorityForm, CertificateAuthority>()
            .ForMember(dest => dest.Parameters, opt => opt.MapFrom(src =>
                JsonSerializer.Serialize(
                    new AcmeConfiguration()
                    {
                        DirectoryUrl = src.DirectoryUrl,
                        Email = src.Email,
                        AcceptTos = true
                    },
                    (JsonSerializerOptions?)null
                )
            ));

        CreateMap<NewApplicationLoadBalancerForm, ApplicationLoadBalancerDTO>();
        CreateMap<NewNetworkLoadBalancerForm, NetworkLoadBalancerDTO>()
            .ForMember(dest => dest.TargetGroup, opt => opt.MapFrom(src => src.TargetGroup));
        CreateMap<NewNetworkLoadBalancerForm, NewApplicationLoadBalancerForm>();
        CreateMap<NewApplicationLoadBalancerForm, NewNetworkLoadBalancerForm>();

        CreateMap<EditApplicationLoadBalancerForm, ApplicationLoadBalancerDTO>();
        CreateMap<EditNetworkLoadBalancerForm, NetworkLoadBalancerDTO>()
            .ForMember(dest => dest.TargetGroup, opt => opt.MapFrom(src => src.TargetGroup));
        CreateMap<ApplicationLoadBalancerDTO, EditApplicationLoadBalancerForm>();
        CreateMap<NetworkLoadBalancerDTO, EditNetworkLoadBalancerForm>()
            .ForMember(dest => dest.TargetGroup, opt => opt.MapFrom(src => src.TargetGroup));
        CreateMap<EditNetworkLoadBalancerForm, EditApplicationLoadBalancerForm>();
        CreateMap<EditApplicationLoadBalancerForm, EditNetworkLoadBalancerForm>();

    }
}
