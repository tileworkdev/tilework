using AutoMapper;
using Tilework.LoadBalancing.Persistence.Models;

namespace Tilework.LoadBalancing.Haproxy;

public static class LoadBalancerToFrontend
{
    private static readonly IMapper Mapper;

    static LoadBalancerToFrontend()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<BaseLoadBalancer, FrontendSection>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForPath(dest => dest.Bind.Address, opt => opt.MapFrom(src => "*"))
                .ForPath(dest => dest.Bind.Port, opt => opt.MapFrom(src => src.Port))
                .AfterMap((src, dest) =>
                {
                    if(src is ApplicationLoadBalancer alb)
                    {
                        dest.Mode = Mode.HTTP;
                        if(alb.Rules != null)
                        {
                            foreach(var rule in alb.Rules)
                            {
                                var usebe = new UseBackend()
                                {
                                    Hostname = rule.Hostname,
                                    Target = rule.TargetGroup.Id.ToString()
                                };
                                dest.UseBackends.Add(usebe);
                            }
                        }

                    }
                    else if(src is NetworkLoadBalancer nlb)
                    {
                        dest.Mode = Mode.TCP;
                        dest.DefaultBackend = nlb.TargetGroup.Id.ToString();
                    }
                });
        });

        Mapper = config.CreateMapper();
    }

    public static FrontendSection Map(BaseLoadBalancer source)
    {
        return Mapper.Map<FrontendSection>(source);
    }
}