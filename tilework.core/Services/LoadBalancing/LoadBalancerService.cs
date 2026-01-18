using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using AutoMapper;


using Tilework.LoadBalancing.Interfaces;
using Tilework.LoadBalancing.Models;
using Tilework.LoadBalancing.Enums;
using Tilework.Persistence.LoadBalancing.Models;
using Tilework.LoadBalancing.Haproxy;

using Tilework.CertificateManagement.Interfaces;
using Tilework.CertificateManagement.Models;

using Tilework.Core.Persistence;


namespace Tilework.LoadBalancing.Services;


public class LoadBalancerService : ILoadBalancerService
{
    private readonly TileworkContext _dbContext;
    private readonly LoadBalancerConfiguration _settings;
    private readonly ILoadBalancingConfigurator _configurator;
    private readonly ILogger<LoadBalancerService> _logger;
    private readonly IMapper _mapper;
    private readonly MonitoringService _monitoringService;


    public LoadBalancerService(IServiceProvider serviceProvider,
                               TileworkContext dbContext,
                               IMapper mapper,
                               IOptions<LoadBalancerConfiguration> settings,
                               ILogger<LoadBalancerService> logger,
                               MonitoringService monitoringService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _settings = settings.Value;
        _configurator = LoadConfigurator(serviceProvider, _settings);
        _monitoringService = monitoringService;
        _mapper = mapper;
    }

    private ILoadBalancingConfigurator LoadConfigurator(IServiceProvider serviceProvider, LoadBalancerConfiguration settings)
    {
        return settings.Backend switch
        {
            "haproxy" => serviceProvider.GetRequiredService<HAProxyConfigurator>(),
            _ => throw new ArgumentException($"Invalid configurator in load balancing tile: {_settings.Backend}")
        };
    }

    private static void ValidateRulePriority(ICollection<Rule> rules, int newPriority)
    {
        if (newPriority < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(newPriority), newPriority, "Rule priority cannot be negative.");
        }

        var maxPriority = rules.Count == 0 ? -1 : rules.Max(r => r.Priority);
        if (newPriority > maxPriority + 1)
        {
            throw new ArgumentOutOfRangeException(nameof(newPriority), newPriority, $"Rule priority cannot be greater than {maxPriority + 1}.");
        }
    }

    private static void EnsureRuleConditionsAllowed(LoadBalancer balancer, RuleDTO rule)
    {
        var invalid = rule.Conditions
            .Select(condition => condition.Type)
            .Distinct()
            .Where(conditionType => !LoadBalancerConditionRules.IsAllowed(balancer.Protocol, conditionType))
            .ToList();

        if (invalid.Count == 0)
        {
            return;
        }

        var invalidList = string.Join(", ", invalid);
        throw new ArgumentException(
            $"Rule contains condition types not supported by {balancer.Protocol}: {invalidList}.",
            nameof(rule));
    }

    private static bool RequiresCertificate(LoadBalancer balancer)
    {
        return balancer.Protocol == LoadBalancerProtocol.HTTPS || balancer.Protocol == LoadBalancerProtocol.TLS;
    }

    private static void EnsureCertificatesPresentIfRequired(LoadBalancer balancer)
    {
        if (RequiresCertificate(balancer) && (balancer.Certificates == null || balancer.Certificates.Count == 0))
        {
            throw new InvalidOperationException($"Load balancer {balancer.Name} requires at least one certificate before it can be enabled.");
        }
    }


    public async Task<List<LoadBalancerDTO>> GetLoadBalancers()
    {
        var entities = await _dbContext.LoadBalancers.ToListAsync();
        return _mapper.Map<List<LoadBalancerDTO>>(entities);
    }

    public async Task<LoadBalancerDTO?> GetLoadBalancer(Guid Id)
    {
        var entity = await _dbContext.LoadBalancers.FindAsync(Id);
        return _mapper.Map<LoadBalancerDTO>(entity);
    }

    public async Task<LoadBalancerDTO> AddLoadBalancer(LoadBalancerDTO balancer)
    {
        EnsureProtocolAllowed(balancer);
        var entity = _mapper.Map<LoadBalancer>(balancer);

        await _dbContext.LoadBalancers.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
        return _mapper.Map<LoadBalancerDTO>(entity);
    }

    public async Task<LoadBalancerDTO> UpdateLoadBalancer(LoadBalancerDTO balancer)
    {
        EnsureProtocolAllowed(balancer);
        var entity = await _dbContext.LoadBalancers.FindAsync(balancer.Id);
        entity = _mapper.Map(balancer, entity);

        _dbContext.LoadBalancers.Update(entity);
        await _dbContext.SaveChangesAsync();

        return _mapper.Map<LoadBalancerDTO>(entity);
    }

    public async Task DeleteLoadBalancer(Guid Id)
    {
        var entity = await _dbContext.LoadBalancers.FindAsync(Id);
        _dbContext.LoadBalancers.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task EnableLoadBalancer(Guid Id)
    {
        var entity = await _dbContext.LoadBalancers
                                     .Include(lb => lb.Certificates)
                                     .FirstOrDefaultAsync(lb => lb.Id == Id);

        if (entity == null)
            throw new ArgumentException($"Load balancer {Id} not found.");

        EnsureCertificatesPresentIfRequired(entity);
        entity.Enabled = true;
        _dbContext.LoadBalancers.Update(entity);

        await using var tx = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            await _dbContext.SaveChangesAsync();
            await ApplyConfiguration(Id);
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            _dbContext.ChangeTracker.Clear();
            throw;
        }
    }

    public async Task DisableLoadBalancer(Guid Id)
    {
        var entity = await _dbContext.LoadBalancers.FindAsync(Id);
        
        entity.Enabled = false;
        _dbContext.LoadBalancers.Update(entity);

        await using var tx = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            await _dbContext.SaveChangesAsync();
            await ApplyConfiguration(Id);
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            _dbContext.ChangeTracker.Clear();
            throw;
        }
    }


    public async Task<List<RuleDTO>> GetRules(LoadBalancerDTO balancer)
    {
        var entity = await _dbContext.LoadBalancers.FindAsync(balancer.Id);
        return _mapper.Map<List<RuleDTO>>(entity.Rules.OrderBy(r => r.Priority));
    }

    public async Task AddRule(LoadBalancerDTO balancer, RuleDTO rule)
    {
        var entity = await _dbContext.LoadBalancers.FindAsync(balancer.Id);
        if (entity == null)
            throw new ArgumentNullException(nameof(balancer));

        ValidateRulePriority(entity.Rules, rule.Priority);
        EnsureRuleConditionsAllowed(entity, rule);
        
        foreach (var existingRule in entity.Rules.Where(r => r.Priority >= rule.Priority))
        {
            existingRule.Priority += 1;
        }

        entity.Rules.Add(_mapper.Map<Rule>(rule));
        _dbContext.LoadBalancers.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateRule(LoadBalancerDTO balancer, RuleDTO rule)
    {
        var entity = await _dbContext.LoadBalancers.FindAsync(balancer.Id);
        if (entity == null)
            throw new ArgumentNullException(nameof(balancer));

        var ruleEntity = entity.Rules.FirstOrDefault(t => t.Id == rule.Id);
        if (ruleEntity == null)
            return;

        ValidateRulePriority(entity.Rules, rule.Priority);
        EnsureRuleConditionsAllowed(entity, rule);

        await using var tx = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var newPriority = rule.Priority;
            var originalPriority = ruleEntity.Priority;
                
            if (newPriority != originalPriority)
            {
                // first move the original rule out of the way in order to not trip a constraint error
                ruleEntity.Priority = -1;
                _dbContext.Entry(ruleEntity).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();


                if (newPriority < originalPriority)
                {
                    // rule moved up. Bump down rules from new priority to old priority
                    var rulesToAdjust = entity.Rules.Where(r => r.Id != rule.Id &&
                                                                r.Priority >= newPriority &&
                                                                r.Priority < originalPriority)
                                                    .OrderByDescending(r => r.Priority);

                    foreach (var ruleToAdjust in rulesToAdjust)
                    {
                        ruleToAdjust.Priority += 1;
                        _dbContext.Entry(ruleToAdjust).State = EntityState.Modified;
                        await _dbContext.SaveChangesAsync();
                    }
                }
                else
                {
                    // rule moved down. Bump up rules from new priority to old priority
                    var rulesToAdjust = entity.Rules.Where(r => r.Id != rule.Id &&
                                                                r.Priority <= newPriority &&
                                                                r.Priority > originalPriority)
                                                    .OrderBy(r => r.Priority);

                    foreach (var ruleToAdjust in rulesToAdjust)
                    {
                        ruleToAdjust.Priority -= 1;
                        _dbContext.Entry(ruleToAdjust).State = EntityState.Modified;
                        await _dbContext.SaveChangesAsync();
                    }
                }
            }

            var moddedRules = entity.Rules.OrderBy(r => r.Priority);

            _mapper.Map(rule, ruleEntity);
            _dbContext.Entry(ruleEntity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            _dbContext.ChangeTracker.Clear();
            throw;
        }
    }

    public async Task RemoveRule(LoadBalancerDTO balancer, RuleDTO rule)
    {
        var entity = await _dbContext.LoadBalancers.FindAsync(balancer.Id);
        if (entity == null)
            throw new ArgumentNullException();

        var ruleEntity = entity.Rules.FirstOrDefault(t => t.Id == rule.Id);
        if (ruleEntity == null)
            return;

        var pri = ruleEntity.Priority;
        entity.Rules.Remove(ruleEntity);

        // bump up lower rules
        foreach (var lowerRule in entity.Rules.Where(r => r.Priority > pri))
        {
            lowerRule.Priority -= 1;
        }
        _dbContext.LoadBalancers.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<CertificateDTO>> GetCertificates(LoadBalancerDTO balancer)
    {
        var entity = await _dbContext.LoadBalancers.FindAsync(balancer.Id);
        return _mapper.Map<List<CertificateDTO>>(entity.Certificates);
    }

    public async Task AddCertificate(LoadBalancerDTO balancer, Guid certificateId)
    {
        var entity = await _dbContext.LoadBalancers.FindAsync(balancer.Id);
        var cert = await _dbContext.Certificates.FindAsync(certificateId);
        entity.Certificates.Add(cert);
        _dbContext.LoadBalancers.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RemoveCertificate(LoadBalancerDTO balancer, Guid certificateId)
    {
        var entity = await _dbContext.LoadBalancers.FindAsync(balancer.Id);
        var c = entity.Certificates.FirstOrDefault(t => t.Id == certificateId);
        entity.Certificates.Remove(c);
        _dbContext.LoadBalancers.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    private static void EnsureProtocolAllowed(LoadBalancerDTO balancer)
    {
        if (!LoadBalancerProtocolRules.IsAllowed(balancer.Type, balancer.Protocol))
        {
            throw new ArgumentException(
                $"Protocol {balancer.Protocol} is not valid for load balancer type {balancer.Type}",
                nameof(balancer));
        }
    }


    public async Task<List<TargetGroupDTO>> GetTargetGroups()
    {
        var entities = await _dbContext.TargetGroups.ToListAsync();
        return _mapper.Map<List<TargetGroupDTO>>(entities);
    }

    public async Task<TargetGroupDTO?> GetTargetGroup(Guid Id)
    {
        var entity = await _dbContext.TargetGroups.FindAsync(Id);
        return _mapper.Map<TargetGroupDTO>(entity);
    }

    public async Task<TargetGroupDTO> AddTargetGroup(TargetGroupDTO group)
    {
        var entity = _mapper.Map<TargetGroup>(group);
        await _dbContext.TargetGroups.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
        return _mapper.Map<TargetGroupDTO>(entity);
    }

    public async Task<TargetGroupDTO> UpdateTargetGroup(TargetGroupDTO group)
    {
        var entity = await _dbContext.TargetGroups.FindAsync(group.Id);
        entity = _mapper.Map(group, entity);

        _dbContext.TargetGroups.Update(entity);
        await _dbContext.SaveChangesAsync();

        return _mapper.Map<TargetGroupDTO>(entity);
    }

    public async Task DeleteTargetGroup(Guid Id)
    {
        var entity = await _dbContext.TargetGroups.FindAsync(Id);
        _dbContext.TargetGroups.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<TargetDTO>> GetTargets(TargetGroupDTO group)
    {
        var entity = await _dbContext.TargetGroups.FindAsync(group.Id);
        return _mapper.Map<List<TargetDTO>>(entity.Targets);
    }

    public async Task<TargetDTO> GetTarget(Guid id)
    {
        var entity = await _dbContext.Targets.FindAsync(id);
        return _mapper.Map<TargetDTO>(entity);
    }

    public async Task AddTarget(TargetGroupDTO group, TargetDTO target)
    {
        var entity = await _dbContext.TargetGroups.FindAsync(group.Id);
        entity.Targets.Add(_mapper.Map<Target>(target));
        _dbContext.TargetGroups.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateTarget(TargetGroupDTO group, TargetDTO target)
    {
        var entity = await _dbContext.TargetGroups.FindAsync(group.Id);
        var t = entity.Targets.FirstOrDefault(t => t.Id == target.Id);
        if (t != null)
        {
            _mapper.Map(target, t);
            _dbContext.TargetGroups.Update(entity);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task RemoveTarget(TargetGroupDTO group, TargetDTO target)
    {
        var entity = await _dbContext.TargetGroups.FindAsync(group.Id);
        var t = entity.Targets.FirstOrDefault(t => t.Id == target.Id);

        entity.Targets.Remove(t);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<LoadBalancerStatus> GetTargetHealth(Guid Id)
    {
        var now = DateTimeOffset.UtcNow;
        var data = await GetTargetStatusData(Id, now.AddMinutes(-5), now);
        var latest = data
            .OrderByDescending(entry => entry.Timestamp)
            .FirstOrDefault();

        return latest?.Status ?? LoadBalancerStatus.UNKNOWN;
    }


    public async Task ApplyConfiguration()
    {
        var balancers = await _dbContext.LoadBalancers.ToListAsync();
        await _configurator.ApplyConfiguration(balancers);
    }

    public async Task ApplyConfiguration(Guid Id)
    {
        var balancer = await _dbContext.LoadBalancers.FindAsync(Id);
        if(balancer != null)
            await _configurator.ApplyConfiguration(balancer);
    }

    public async Task Shutdown()
    {
        await _configurator.Shutdown();
    }

    public async Task<List<LoadBalancingMonitorData>> GetLoadBalancerMonitoringData(Guid id, TimeSpan interval, DateTimeOffset start, DateTimeOffset end)
    {
        var lb = await GetLoadBalancer(id);
        if(lb == null)
            throw new ArgumentException("Invalid load balancer id");

        var filters = new Dictionary<string, string>();
        filters["type"] = "frontend";
        filters["instance"] = lb.Id.ToString();

        return await _monitoringService.GetMonitoringData<LoadBalancingMonitorData>("LoadBalancing", filters, interval, start, end);
    }

    public async Task<List<LoadBalancingMonitorData>> GetTargetMonitoringData(Guid id, TimeSpan interval, DateTimeOffset start, DateTimeOffset end)
    {
        var tg = await GetTarget(id);
        if(tg == null)
            throw new ArgumentException("Invalid target id");

        var filters = new Dictionary<string, string>();
        filters["type"] = "server";
        filters["sv"] = tg.Id.ToString();

        return await _monitoringService.GetMonitoringData<LoadBalancingMonitorData>("LoadBalancing", filters, interval, start, end);
    }

    public async Task<List<LoadBalancingStatusData>> GetTargetStatusData(Guid id, DateTimeOffset start, DateTimeOffset end)
    {
        var tg = await GetTarget(id);
        if(tg == null)
            throw new ArgumentException("Invalid target id");

        var filters = new Dictionary<string, string>();
        filters["type"] = "server";
        filters["sv"] = tg.Id.ToString();

        return await _monitoringService.GetMonitoringData<LoadBalancingStatusData>("LoadBalancing", filters, null, start, end);
    }
}
