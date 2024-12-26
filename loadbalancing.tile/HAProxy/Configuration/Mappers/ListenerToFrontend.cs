using AutoMapper;
using Tilework.LoadBalancing.Persistence.Models;

namespace Tilework.LoadBalancing.Haproxy;

public static class ListenerToFrontend
{
    private static readonly IMapper Mapper;

    static ListenerToFrontend()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Listener, FrontendSection>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.DefaultBackend, opt => opt.MapFrom(src => src.Rules[0].Id.ToString()))
                .ForMember(dest => dest.Mode, opt => opt.MapFrom(src => src.LoadBalancer.Type))
                .ForPath(dest => dest.Bind.Address, opt => opt.MapFrom(src => "*"))
                .ForPath(dest => dest.Bind.Port, opt => opt.MapFrom(src => src.Port))
                .AfterMap((src, dest) =>
                {
                    dest.Mode = src.LoadBalancer.Type == Enums.LoadBalancerType.APPLICATION ? Mode.HTTP : Mode.TCP;
                });
        });

        Mapper = config.CreateMapper();
    }

    public static FrontendSection Map(Listener source)
    {
        return Mapper.Map<FrontendSection>(source);
    }
}