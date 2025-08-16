using AutoMapper;

using Tilework.LoadBalancing.Persistence.Models;
using Tilework.Core.Models;
using Tilework.Core.LoadBalancing.Models;

namespace Tilework.LoadBalancing.Mappers;

public class LoadBalancingMappingProfile : Profile
{
    public LoadBalancingMappingProfile()
    {
        CreateMap<ApplicationLoadBalancer, ApplicationLoadBalancerDTO>()
            .ForMember(dest => dest.Certificates, opt => opt.MapFrom(src => src.CertificateIds));
        CreateMap<ApplicationLoadBalancerDTO, ApplicationLoadBalancer>()
            .ForMember(dest => dest.CertificateIds, opt => opt.MapFrom(src => src.Certificates));
        

        CreateMap<NetworkLoadBalancer, NetworkLoadBalancerDTO>()
            .ForMember(dest => dest.Certificates, opt => opt.MapFrom(src => src.CertificateIds))
            .ForMember(dest => dest.TargetGroup, opt => opt.MapFrom(src => src.TargetGroupId));
        CreateMap<NetworkLoadBalancerDTO, NetworkLoadBalancer>()
            .ForMember(dest => dest.CertificateIds, opt => opt.MapFrom(src => src.Certificates))
            .ForMember(dest => dest.TargetGroupId, opt => opt.MapFrom(src => src.TargetGroup))
            .ForMember(dest => dest.TargetGroup, opt => opt.Ignore());

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
