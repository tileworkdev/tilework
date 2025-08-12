using AutoMapper;

using Tilework.Ui.Models;
using Tilework.LoadBalancing.Persistence.Models;
using Tilework.CertificateManagement.Persistence.Models;

namespace Tilework.Ui.Mappers;

public class FormMappingProfile : Profile
{
    public FormMappingProfile()
    {
        CreateMap<NewTargetGroupForm, TargetGroup>();
        CreateMap<EditTargetGroupForm, TargetGroup>();
        CreateMap<TargetGroup, EditTargetGroupForm>();

        CreateMap<NewCertificateAuthorityForm, CertificateAuthority>();

        CreateMap<NewApplicationLoadBalancerForm, ApplicationLoadBalancer>();
        CreateMap<NewNetworkLoadBalancerForm, NetworkLoadBalancer>()
            .ForMember(dest => dest.TargetGroupId, opt => opt.MapFrom(src => src.TargetGroup))
            .ForMember(dest => dest.TargetGroup, opt => opt.Ignore());
        CreateMap<NewNetworkLoadBalancerForm, NewApplicationLoadBalancerForm>();
        CreateMap<NewApplicationLoadBalancerForm, NewNetworkLoadBalancerForm>();

        CreateMap<EditApplicationLoadBalancerForm, ApplicationLoadBalancer>();
        CreateMap<EditNetworkLoadBalancerForm, NetworkLoadBalancer>()
            .ForMember(dest => dest.TargetGroupId, opt => opt.MapFrom(src => src.TargetGroup))
            .ForMember(dest => dest.TargetGroup, opt => opt.Ignore());
        CreateMap<ApplicationLoadBalancer, EditApplicationLoadBalancerForm>();
        CreateMap<NetworkLoadBalancer, EditNetworkLoadBalancerForm>()
            .ForMember(dest => dest.TargetGroup, opt => opt.MapFrom(src => src.TargetGroupId));
        CreateMap<EditNetworkLoadBalancerForm, EditApplicationLoadBalancerForm>();
        CreateMap<EditApplicationLoadBalancerForm, EditNetworkLoadBalancerForm>();

    }
}
