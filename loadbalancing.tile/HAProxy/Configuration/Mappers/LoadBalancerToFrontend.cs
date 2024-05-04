using AutoMapper;
using Tilework.LoadBalancing.Models;

namespace Tilework.LoadBalancing.Haproxy;

public static class LoadBalancerToFrontend
{
    private static readonly IMapper Mapper;

    static LoadBalancerToFrontend()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<LoadBalancer, FrontendSection>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.DefaultBackend, opt => opt.MapFrom(src => src.Group.Id.ToString()))
                .ForMember(dest => dest.Mode, opt => opt.MapFrom(src => src.Type))
                .ForPath(dest => dest.Bind.Address, opt => opt.MapFrom(src => "*"))
                .ForPath(dest => dest.Bind.Port, opt => opt.MapFrom(src => src.Port))
                .AfterMap((src, dest) =>
                {
                    dest.Mode = src.Type == Enums.LoadBalancerType.APPLICATION ? Mode.HTTP : Mode.TCP;
                });
        });

        Mapper = config.CreateMapper();
    }

    public static FrontendSection Map(LoadBalancer source)
    {
        return Mapper.Map<FrontendSection>(source);
    }
}