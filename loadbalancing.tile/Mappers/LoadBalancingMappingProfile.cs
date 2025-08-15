using AutoMapper;

using Tilework.LoadBalancing.Persistence.Models;
using Tilework.Core.Models;
using Tilework.Core.LoadBalancing.Models;

namespace Tilework.LoadBalancing.Mappers;

public class LoadBalancingMappingProfile : Profile
{
    public LoadBalancingMappingProfile()
    {
        CreateMap<ApplicationLoadBalancerDTO, ApplicationLoadBalancer>();
        CreateMap<ApplicationLoadBalancer, ApplicationLoadBalancerDTO>();

        CreateMap<NetworkLoadBalancer, NetworkLoadBalancerDTO>()
            .ForMember(dest => dest.TargetGroup, opt => opt.MapFrom(src => src.TargetGroupId));
        CreateMap<NetworkLoadBalancerDTO, NetworkLoadBalancer>()
            .ForMember(dest => dest.TargetGroupId, opt => opt.MapFrom(src => src.TargetGroup))
            .ForMember(dest => dest.TargetGroup, opt => opt.Ignore());

        CreateMap<TargetGroupDTO, TargetGroup>();
        CreateMap<TargetGroup, TargetGroupDTO>();

        CreateMap<TargetDTO, Target>();
        CreateMap<Target, TargetDTO>();
    }
}
