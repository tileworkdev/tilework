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
using Tilework.Persistence.CertificateManagement.Models;
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
    private readonly ICertificateManagementService _certificateManagementService;


    public LoadBalancerService(IServiceProvider serviceProvider,
                               TileworkContext dbContext,
                               IMapper mapper,
                               ICertificateManagementService certificateManagementService,
                               IOptions<LoadBalancerConfiguration> settings,
                               ILogger<LoadBalancerService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _settings = settings.Value;
        _configurator = LoadConfigurator(serviceProvider, _settings);

        _certificateManagementService = certificateManagementService;
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

    private BaseLoadBalancerDTO MapBalancerToDto(BaseLoadBalancer entity)
    {
        return entity switch
        {
            ApplicationLoadBalancer appBalancer => _mapper.Map<ApplicationLoadBalancerDTO>(appBalancer),
            NetworkLoadBalancer netBalancer => _mapper.Map<NetworkLoadBalancerDTO>(netBalancer),
            _ => throw new InvalidOperationException("Invalid balancer type")
        };
    }

    private BaseLoadBalancer MapDtoToBalancer(BaseLoadBalancerDTO dto, BaseLoadBalancer? entity = null)
    {
        return dto switch
        {
            ApplicationLoadBalancerDTO appBalancer =>
                entity == null ? _mapper.Map<ApplicationLoadBalancer>(appBalancer) : _mapper.Map(appBalancer, (ApplicationLoadBalancer) entity),

            NetworkLoadBalancerDTO netBalancer =>
                entity == null ? _mapper.Map<NetworkLoadBalancer>(netBalancer) : _mapper.Map(netBalancer, (NetworkLoadBalancer) entity),

            _ => throw new InvalidOperationException("Invalid balancer type")
        };
    }

    public async Task<List<BaseLoadBalancerDTO>> GetLoadBalancers()
    {
        var entities = await _dbContext.LoadBalancers.ToListAsync();
        return entities.Select(e => MapBalancerToDto(e)).ToList();
    }

    public async Task<BaseLoadBalancerDTO?> GetLoadBalancer(Guid Id)
    {
        var entity = await _dbContext.LoadBalancers.FindAsync(Id);
        return entity != null ? MapBalancerToDto(entity) : null;
    }

    public async Task<BaseLoadBalancerDTO> AddLoadBalancer(BaseLoadBalancerDTO balancer)
    {
        var entity = MapDtoToBalancer(balancer);

        await _dbContext.LoadBalancers.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
        return MapBalancerToDto(entity);
    }

    public async Task<BaseLoadBalancerDTO> UpdateLoadBalancer(BaseLoadBalancerDTO balancer)
    {
        var entity = await _dbContext.LoadBalancers.FindAsync(balancer.Id);
        entity = MapDtoToBalancer(balancer, entity);

        _dbContext.LoadBalancers.Update(entity);
        await _dbContext.SaveChangesAsync();
        
        return MapBalancerToDto(entity);
    }

    public async Task DeleteLoadBalancer(Guid Id)
    {
        var entity = await _dbContext.LoadBalancers.FindAsync(Id);
        _dbContext.LoadBalancers.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }


    public async Task<List<RuleDTO>> GetRules(ApplicationLoadBalancerDTO balancer)
    {
        var entity = (ApplicationLoadBalancer?) await _dbContext.LoadBalancers.FindAsync(balancer.Id);
        return _mapper.Map<List<RuleDTO>>(entity.Rules);
    }

    public async Task AddRule(ApplicationLoadBalancerDTO balancer, RuleDTO rule)
    {
        var entity = (ApplicationLoadBalancer?) await _dbContext.LoadBalancers.FindAsync(balancer.Id);
        entity.Rules.Add(_mapper.Map<Rule>(rule));
        _dbContext.LoadBalancers.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateRule(ApplicationLoadBalancerDTO balancer, RuleDTO rule)
    {
        var entity = (ApplicationLoadBalancer?) await _dbContext.LoadBalancers.FindAsync(balancer.Id);
        var r = entity.Rules.FirstOrDefault(t => t.Id == rule.Id);
        if (r != null)
        {
            _mapper.Map(rule, r);
            _dbContext.LoadBalancers.Update(entity);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task RemoveRule(ApplicationLoadBalancerDTO balancer, RuleDTO rule)
    {
        var entity = (ApplicationLoadBalancer?) await _dbContext.LoadBalancers.FindAsync(balancer.Id);
        var r = entity.Rules.FirstOrDefault(t => t.Id == rule.Id);
        entity.Rules.Remove(r);
        _dbContext.LoadBalancers.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<CertificateDTO>> GetCertificates(BaseLoadBalancerDTO balancer)
    {
        var entity = await _dbContext.LoadBalancers.FindAsync(balancer.Id);
        return _mapper.Map<List<CertificateDTO>>(entity.Certificates);
    }

    public async Task AddCertificate(BaseLoadBalancerDTO balancer, Guid certificateId)
    {
        var entity = await _dbContext.LoadBalancers.FindAsync(balancer.Id);
        var cert = await _dbContext.Certificates.FindAsync(certificateId);
        entity.Certificates.Add(cert);
        _dbContext.LoadBalancers.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RemoveCertificate(BaseLoadBalancerDTO balancer, Guid certificateId)
    {
        var entity = await _dbContext.LoadBalancers.FindAsync(balancer.Id);
        var c = entity.Certificates.FirstOrDefault(t => t.Id == certificateId);
        entity.Certificates.Remove(c);
        _dbContext.LoadBalancers.Update(entity);
        await _dbContext.SaveChangesAsync();
    }


    public async Task<List<TargetGroupDTO>> GetTargetGroups()
    {
        var entities = await _dbContext.TargetGroups.ToListAsync();
        return _mapper.Map<List<TargetGroupDTO>>(entities);
    }

    public async Task<List<TargetGroupDTO>> GetNlbTargetGroups()
    {
        var protocols = new List<TargetGroupProtocol> {
            TargetGroupProtocol.TCP,
            TargetGroupProtocol.UDP,
            TargetGroupProtocol.TCP_UDP,
            TargetGroupProtocol.TLS
        };
        return (await GetTargetGroups()).Where(tg => protocols.Contains(tg.Protocol)).ToList();
    }

    public async Task<List<TargetGroupDTO>> GetAlbTargetGroups()
    {
        var protocols = new List<TargetGroupProtocol> {
            TargetGroupProtocol.HTTP,
            TargetGroupProtocol.HTTPS
        };
        return (await GetTargetGroups()).Where(tg => protocols.Contains(tg.Protocol)).ToList();
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


    public async Task ApplyConfiguration()
    {
        var balancers = await _dbContext.LoadBalancers.ToListAsync();
        await _configurator.ApplyConfiguration(balancers);
    }

    public async Task Shutdown()
    {
        await _configurator.Shutdown();
    }
}
