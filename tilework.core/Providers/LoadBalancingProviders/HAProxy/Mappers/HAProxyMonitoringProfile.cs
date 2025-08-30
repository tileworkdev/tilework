using AutoMapper;
using Tilework.LoadBalancing.Models;

namespace Tilework.LoadBalancing.Haproxy;

public class HAProxyMonitoringProfile : Profile
{
    public HAProxyMonitoringProfile()
    {
        CreateMap<(HAProxyInfo, HAProxyStatisticsRow), LoadBalancingStatistics>()
            .ForMember(dest => dest.Uptime, opt => opt.MapFrom(src => TimeSpan.FromSeconds(Int32.Parse(src.Item1.Uptime_sec))))
            .ForMember(dest => dest.TotalSessions, opt => opt.MapFrom(src => src.Item2.stot));
            // .ForMember(dest => dest.CurrentSessions, opt => opt.MapFrom(src => src.scur))
            // .ForPath(dest => dest.CurrentQueue, opt => opt.MapFrom(src => src.qcur));
    }
}

