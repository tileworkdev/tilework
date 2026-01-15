using AutoMapper;

using Tilework.Persistence.LoadBalancing.Models;
using Tilework.Core.Models;
using Tilework.Core.Mappers;
using Tilework.LoadBalancing.Enums;
using Tilework.LoadBalancing.Models;

namespace Tilework.LoadBalancing.Mappers;

public class LoadBalancingMappingProfile : Profile
{
    public LoadBalancingMappingProfile()
    {
        CreateMap<string, LoadBalancerStatus>()
            .ConvertUsing(status => EnumMappers.FromHaproxyStatus(status));

        CreateMap<ApplicationLoadBalancer, ApplicationLoadBalancerDTO>();
        CreateMap<ApplicationLoadBalancerDTO, ApplicationLoadBalancer>()
            .ForMember(dest => dest.Enabled, opt => opt.Ignore());
        

        CreateMap<NetworkLoadBalancer, NetworkLoadBalancerDTO>()
            .ForMember(dest => dest.TargetGroup, opt => opt.MapFrom(src => src.TargetGroupId));
        CreateMap<NetworkLoadBalancerDTO, NetworkLoadBalancer>()
            .ForMember(dest => dest.TargetGroupId, opt => opt.MapFrom(src => src.TargetGroup))
            .ForMember(dest => dest.TargetGroup, opt => opt.Ignore())
            .ForMember(dest => dest.Enabled, opt => opt.Ignore());

        CreateMap<TargetGroupDTO, TargetGroup>();
        CreateMap<TargetGroup, TargetGroupDTO>();

        CreateMap<TargetDTO, Target>();
        CreateMap<Target, TargetDTO>();

        CreateMap<Rule, RuleDTO>()
            .ForMember(dest => dest.LoadBalancer, opt => opt.MapFrom(src => src.LoadBalancerId))
            .ForMember(dest => dest.TargetGroup, opt => opt.MapFrom(src => src.TargetGroupId));
        CreateMap<RuleDTO, Rule>()
            .ForMember(dest => dest.LoadBalancerId, opt => opt.MapFrom(src => src.LoadBalancer))
            .ForMember(dest => dest.TargetGroupId, opt => opt.MapFrom(src => src.TargetGroup))
            .ForMember(dest => dest.LoadBalancer, opt => opt.Ignore())
            .ForMember(dest => dest.TargetGroup, opt => opt.Ignore());
    }
}
