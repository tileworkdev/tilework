using AutoMapper;
using Tilework.LoadBalancing.Enums;
using Tilework.LoadBalancing.Persistence.Models;

namespace Tilework.LoadBalancing.Haproxy;

public static class TargetGroupToBackend
{
    private static readonly IMapper Mapper;

    static TargetGroupToBackend()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<TargetGroup, BackendSection>()
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Id))
               .AfterMap((src, dest) => {

                    dest.Mode =  src.Protocol switch
                    {
                        TargetGroupProtocol.HTTP => Mode.HTTP,
                        TargetGroupProtocol.HTTPS => Mode.HTTP,
                        TargetGroupProtocol.TCP => Mode.TCP,
                        _ => throw new NotImplementedException()
                    };

                    dest.Servers = src.Targets.Select(target =>
                        new Server() {
                            Name = target.Id.ToString(),
                            Address = target.Address.ToString(),
                            Port = target.Port,
                            Check = true
                        }
                    ).ToList();
               });
        });

        Mapper = config.CreateMapper();
    }

    public static BackendSection Map(TargetGroup source)
    {
        return Mapper.Map<BackendSection>(source);
    }
}