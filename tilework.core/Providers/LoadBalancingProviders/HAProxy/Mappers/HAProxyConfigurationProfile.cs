using AutoMapper;

using Tilework.Core.Enums;
using Tilework.LoadBalancing.Enums;
using Tilework.Persistence.LoadBalancing.Models;

namespace Tilework.LoadBalancing.Haproxy;

public class HAProxyConfigurationProfile : Profile
{
    private const string BackendVariableName = "txn.tilework_backend";
    private const string BackendSelectedAclName = "backend_selected";

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

                dest.Mode = src.Type == LoadBalancerType.APPLICATION ? Mode.HTTP : Mode.TCP;
                if (dest.Mode == Mode.HTTP)
                    MapHttpLoadBalancerRules(src, dest, context);
                else
                    MapTcpLoadBalancerRules(src, dest, context);
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
            ConditionType.SourceIp => AclCondition.SourceIp,
            _ => throw new NotSupportedException($"Unsupported condition type for HAProxy ACL mapping: {conditionType}")
        };
    }

    private static void MapHttpLoadBalancerRules(LoadBalancer src, FrontendSection dest, ResolutionContext context)
    {
        dest.HttpRequests.Add(new AddHeaderHttpRequest(
            "X-Forwarded-Proto",
            src.Protocol == LoadBalancerProtocol.HTTPS ? "https" : "http"));

        dest.HttpRequests.Add(new AddHeaderHttpRequest(
            "X-Forwarded-Port",
            src.Port.ToString()));

        if (src.Rules is not { Count: > 0 })
            return;

        var hasForwardRule = false;
        dest.Acls.Add(new Acl()
        {
            Name = BackendSelectedAclName,
            Type = AclCondition.VariableSet,
            Values = new List<string> { BackendVariableName }
        });

        foreach (var rule in src.Rules.OrderBy(r => r.Priority))
        {
            var ruleAcls = BuildRuleAcls(rule, context);
            dest.Acls.AddRange(ruleAcls);

            if (rule.Action == null)
                throw new InvalidOperationException($"Rule {rule.Id} is missing an action.");

            var gatedAcls = new List<string> { $"!{BackendSelectedAclName}" };
            gatedAcls.AddRange(ruleAcls.Select(a => a.Name));

            switch (rule.Action.Type)
            {
                case RuleActionType.Forward:
                    if (rule.TargetGroup != null)
                    {
                        hasForwardRule = true;
                        dest.HttpRequests.Add(new SetVariableHttpRequest(BackendVariableName, rule.TargetGroup.Id.ToString())
                        {
                            Acls = gatedAcls
                        });
                    }
                    break;
                case RuleActionType.Redirect:
                    if (string.IsNullOrWhiteSpace(rule.Action.RedirectUrl))
                        throw new InvalidOperationException($"Rule {rule.Id} redirect action is missing RedirectUrl.");

                    dest.HttpRequests.Add(new RedirectHttpRequest(rule.Action.RedirectUrl)
                    {
                        StatusCode = rule.Action.RedirectStatusCode,
                        Acls = gatedAcls
                    });
                    break;
                case RuleActionType.FixedResponse:
                    if (rule.Action.FixedResponseStatusCode == null)
                        throw new InvalidOperationException($"Rule {rule.Id} fixed response action is missing FixedResponseStatusCode.");

                    dest.HttpRequests.Add(new ReturnHttpRequest(rule.Action.FixedResponseStatusCode.Value)
                    {
                        ContentType = rule.Action.FixedResponseContentType,
                        Body = rule.Action.FixedResponseBody,
                        Acls = gatedAcls
                    });
                    break;
                case RuleActionType.Reject:
                    dest.HttpRequests.Add(new DenyHttpRequest()
                    {
                        Acls = gatedAcls
                    });
                    break;
                default:
                    throw new NotSupportedException($"Unsupported rule action: {rule.Action.Type}");
            }
        }

        if (hasForwardRule)
        {
            dest.UseBackends.Add(new UseBackend()
            {
                Target = $"%[var({BackendVariableName})]",
                Acls = new List<string> { BackendSelectedAclName }
            });
        }
    }

    private static void MapTcpLoadBalancerRules(LoadBalancer src, FrontendSection dest, ResolutionContext context)
    {
        if (src.Rules is not { Count: > 0 })
            return;

        foreach (var rule in src.Rules.OrderBy(r => r.Priority))
        {
            var ruleAcls = BuildRuleAcls(rule, context);
            dest.Acls.AddRange(ruleAcls);

            if (rule.Action == null)
                throw new InvalidOperationException($"Rule {rule.Id} is missing an action.");

            var aclNames = ruleAcls.Select(a => a.Name).ToList();
            switch (rule.Action.Type)
            {
                case RuleActionType.Forward:
                    if (rule.TargetGroup != null)
                    {
                        dest.UseBackends.Add(new UseBackend()
                        {
                            Acls = aclNames,
                            Target = rule.TargetGroup.Id.ToString()
                        });
                    }
                    break;
                case RuleActionType.Reject:
                    dest.TcpRequests.Add(new TcpRequest()
                    {
                        Acls = aclNames
                    });
                    break;
                default:
                    throw new NotSupportedException($"Unsupported TCP rule action: {rule.Action.Type}");
            }
        }
    }

    private static List<Acl> BuildRuleAcls(Rule rule, ResolutionContext context)
    {
        var ruleAcls = new List<Acl>();
        for (int i = 0; i < rule.Conditions.Count; i++)
        {
            var condition = rule.Conditions[i];
            ruleAcls.Add(new Acl()
            {
                Name = $"{rule.Id.ToString()}-{i}",
                Type = context.Mapper.Map<AclCondition>(condition.Type),
                Values = condition.Values
            });
        }

        return ruleAcls;
    }
}
