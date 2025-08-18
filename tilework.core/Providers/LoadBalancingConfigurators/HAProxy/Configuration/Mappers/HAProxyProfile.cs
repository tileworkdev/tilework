using AutoMapper;

using Tilework.Core.Enums;
using Tilework.LoadBalancing.Enums;
using Tilework.Persistence.LoadBalancing.Models;

namespace Tilework.LoadBalancing.Haproxy;

public class HAProxyProfile : Profile
{
    public HAProxyProfile()
    {
        CreateMap<BaseLoadBalancer, FrontendSection>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForPath(dest => dest.Bind.Address, opt => opt.MapFrom(src => "*"))
            .ForPath(dest => dest.Bind.Port, opt => opt.MapFrom(src => src.Port))
            .AfterMap((src, dest) =>
            {
                if (src is ApplicationLoadBalancer alb)
                {
                    if (alb.Protocol == AlbProtocol.HTTPS)
                        dest.Bind.EnableTls = true;

                    dest.Mode = Mode.HTTP;
                    if (alb.Rules != null)
                    {
                        foreach (var rule in alb.Rules.OrderBy(r => r.Priority))
                        {
                            var acls = new List<Acl>();
                            for (int i = 0; i < rule.Conditions.Count; i++)
                            {
                                var condition = rule.Conditions[i];

                                var acl = new Acl()
                                {
                                    Name = $"{rule.Id.ToString()}-{i}",
                                    Type = condition.Type,
                                    Values = condition.Values
                                };

                                acls.Add(acl);
                            }

                            dest.Acls.AddRange(acls);

                            var usebe = new UseBackend()
                            {
                                Acls = acls.Select(a => a.Name).ToList(),
                                Target = rule.TargetGroup.Id.ToString(),
                            };
                            dest.UseBackends.Add(usebe);
                        }
                    }
                }
                else if (src is NetworkLoadBalancer nlb)
                {
                    if (nlb.Protocol == NlbProtocol.TLS)
                        dest.Bind.EnableTls = true;

                    dest.Mode = Mode.TCP;
                    dest.DefaultBackend = nlb.TargetGroup.Id.ToString();
                }
            });

        CreateMap<BaseLoadBalancer, PortType>()
            .ConvertUsing((BaseLoadBalancer src) =>
                src is NetworkLoadBalancer && ((NetworkLoadBalancer)src).Protocol == NlbProtocol.UDP
                    ? PortType.UDP
                    : PortType.TCP);

        CreateMap<TargetGroup, BackendSection>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Id))
            .AfterMap((src, dest) =>
            {
                dest.Mode = src.Protocol switch
                {
                    TargetGroupProtocol.HTTP => Mode.HTTP,
                    TargetGroupProtocol.HTTPS => Mode.HTTP,
                    TargetGroupProtocol.TCP => Mode.TCP,
                    _ => throw new NotImplementedException(),
                };

                dest.Servers = src.Targets.Select(target => new Server()
                {
                    Name = target.Id.ToString(),
                    Address = target.Host.Value,
                    Port = target.Port,
                    Check = true,
                }).ToList();
            });
    }
}

