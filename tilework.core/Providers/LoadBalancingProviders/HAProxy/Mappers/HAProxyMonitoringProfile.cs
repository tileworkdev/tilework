using AutoMapper;
using Tilework.LoadBalancing.Models;

namespace Tilework.LoadBalancing.Haproxy;

public class HAProxyMonitoringProfile : Profile
{
    public HAProxyMonitoringProfile()
    {
        CreateMap<HAProxyStatisticsRow, LoadBalancingStatistics>()
            .ForMember(dest => dest.CurrentSessions, opt => opt.MapFrom(src => src.scur))
            .ForPath(dest => dest.CurrentQueue, opt => opt.MapFrom(src => src.qcur));
    }
}

