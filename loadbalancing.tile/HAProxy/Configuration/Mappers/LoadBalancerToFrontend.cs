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
                // .ForMember(dest => dest.DefaultBackend, opt => opt.MapFrom(src => src.Rules[0].Id.ToString()))
                .ForPath(dest => dest.Bind.Address, opt => opt.MapFrom(src => "*"))
                .ForPath(dest => dest.Bind.Port, opt => opt.MapFrom(src => src.Port))
                .AfterMap((src, dest) =>
                {
                    dest.Mode = src is ApplicationLoadBalancer ? Mode.HTTP : Mode.TCP;
                });
        });

        Mapper = config.CreateMapper();
    }

    public static FrontendSection Map(BaseLoadBalancer source)
    {
        return Mapper.Map<FrontendSection>(source);
    }
}