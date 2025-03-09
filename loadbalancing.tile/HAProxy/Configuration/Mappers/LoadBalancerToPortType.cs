using AutoMapper;
using Tilework.Core.Enums;
using Tilework.LoadBalancing.Persistence.Models;
using Tilework.LoadBalancing.Enums;

namespace Tilework.LoadBalancing.Haproxy;

public static class LoadBalancerToPortType
{
    private static readonly IMapper Mapper;

    static LoadBalancerToPortType()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<BaseLoadBalancer, PortType>()
               .AfterMap((src, dest) => {
                if(src is ApplicationLoadBalancer alb)
                {
                    dest = PortType.TCP;
                }
                else if(src is NetworkLoadBalancer nlb)
                {
                    switch(nlb.Protocol)
                    {
                        case NlbProtocol.TCP:
                        case NlbProtocol.TLS:
                            dest = PortType.TCP;
                            break;
                        case NlbProtocol.UDP:
                            dest = PortType.UDP;
                            break;
                    }
                }

               });
        });

        Mapper = config.CreateMapper();
    }

    public static PortType Map(BaseLoadBalancer source)
    {
        return Mapper.Map<PortType>(source);
    }
}