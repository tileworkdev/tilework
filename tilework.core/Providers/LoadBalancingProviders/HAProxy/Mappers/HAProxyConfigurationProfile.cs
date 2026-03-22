using AutoMapper;

using Tilework.Core.Enums;
using Tilework.LoadBalancing.Enums;
using Tilework.Persistence.LoadBalancing.Models;

namespace Tilework.LoadBalancing.Haproxy;

public class HAProxyConfigurationProfile : Profile
{
    public HAProxyConfigurationProfile()
    {
        CreateMap<ConditionType, AclCondition>()
            .ConvertUsing(src => MapToAclCondition(src));

        CreateMap<LoadBalancer, FrontendSection>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForPath(dest => dest.Bind.Address, opt => opt.MapFrom(src => "*"))
            .ForPath(dest => dest.Bind.Port, opt => opt.MapFrom(src => src.Port))
            .AfterMap((src, dest, context) =>
            {
                if (src.Protocol == LoadBalancerProtocol.HTTPS || src.Protocol == LoadBalancerProtocol.TLS)
                    dest.Bind.EnableTls = true;

                if (src.Type == LoadBalancerType.APPLICATION)
                {
                    dest.AddHeaders.Add(new HttpHeader()
                    {
                        Name = "X-Forwarded-Proto",
                        Value = src.Protocol == LoadBalancerProtocol.HTTPS ? "https" : "http"
                    });

                    dest.AddHeaders.Add(new HttpHeader()
                    {
                        Name = "X-Forwarded-Port",
                        Value = src.Port.ToString()
                    });
                }

                dest.Mode = src.Type == LoadBalancerType.APPLICATION ? Mode.HTTP : Mode.TCP;
                if (src.Rules != null)
                {
                    foreach (var rule in src.Rules.OrderBy(r => r.Priority))
                    {
                        var acls = new List<Acl>();
                        for (int i = 0; i < rule.Conditions.Count; i++)
                        {
                            var condition = rule.Conditions[i];

                            var acl = new Acl()
                            {
                                Name = $"{rule.Id.ToString()}-{i}",
                                Type = context.Mapper.Map<AclCondition>(condition.Type),
                                Values = condition.Values
                            };

                            acls.Add(acl);
                        }

                        dest.Acls.AddRange(acls);

                        if (rule.Action == null)
                            throw new InvalidOperationException($"Rule {rule.Id} is missing an action.");

                        var actionType = rule.Action.Type;
                        switch (actionType)
                        {
                            case RuleActionType.Forward:
                                var targetGroup = rule.TargetGroup;
                                if (targetGroup != null)
                                {
                                    var usebe = new UseBackend()
                                    {
                                        Acls = acls.Select(a => a.Name).ToList(),
                                        Target = targetGroup.Id.ToString(),
                                    };
                                    dest.UseBackends.Add(usebe);
                                }
                                break;
                            case RuleActionType.Redirect:
                                if (string.IsNullOrWhiteSpace(rule.Action.RedirectUrl))
                                    throw new InvalidOperationException($"Rule {rule.Id} redirect action is missing RedirectUrl.");

                                dest.HttpRequests.Add(new RedirectHttpRequest(rule.Action.RedirectUrl)
                                {
                                    StatusCode = rule.Action.RedirectStatusCode,
                                    Acls = acls.Select(a => a.Name).ToList()
                                });
                                break;
                            case RuleActionType.FixedResponse:
                                if (rule.Action.FixedResponseStatusCode == null)
                                    throw new InvalidOperationException($"Rule {rule.Id} fixed response action is missing FixedResponseStatusCode.");

                                dest.HttpRequests.Add(new ReturnHttpRequest(rule.Action.FixedResponseStatusCode.Value)
                                {
                                    ContentType = rule.Action.FixedResponseContentType,
                                    Body = rule.Action.FixedResponseBody,
                                    Acls = acls.Select(a => a.Name).ToList()
                                });
                                break;
                            case RuleActionType.Reject:
                                dest.TcpRequests.Add(new TcpRequest()
                                {
                                    Acls = acls.Select(a => a.Name).ToList()
                                });
                                break;
                            default:
                                throw new NotSupportedException($"Unsupported rule action: {actionType}");
                        }
                    }
                }
            });

        CreateMap<LoadBalancer, PortType>()
            .ConvertUsing(src =>
                src.Protocol == LoadBalancerProtocol.UDP
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
                    TargetGroupProtocol.UDP => Mode.TCP,
                    TargetGroupProtocol.TCP_UDP => Mode.TCP,
                    TargetGroupProtocol.TLS => Mode.TCP,
                    _ => throw new NotImplementedException(),
                };

                dest.Servers = src.Targets.Select(target => new Server()
                {
                    Name = target.Id.ToString(),
                    Address = target.Host.Value,
                    Port = target.Port,
                    Check = true,
                    Tls = src.Protocol == TargetGroupProtocol.HTTPS || src.Protocol == TargetGroupProtocol.TLS
                }).ToList();
            });
    }

    private static AclCondition MapToAclCondition(ConditionType conditionType)
    {
        return conditionType switch
        {
            ConditionType.HostHeader => AclCondition.HostHeader,
            ConditionType.Path => AclCondition.Path,
            ConditionType.QueryString => AclCondition.QueryString,
            ConditionType.SNI => AclCondition.SNI,
            _ => throw new NotSupportedException($"Unsupported condition type for HAProxy ACL mapping: {conditionType}")
        };
    }
}
