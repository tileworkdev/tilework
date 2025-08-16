using AutoMapper;

using Tilework.Ui.Models;
using Tilework.Core.CertificateManagement.Models;
using Tilework.Core.LoadBalancing.Models;

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


        // Certificate authorities
        CreateMap<NewAcmeCertificateAuthorityForm, CertificateAuthorityDTO>()
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

    }
}
