using Tilework.LoadBalancing.Models;
using Tilework.CertificateManagement.Models;
using Tilework.LoadBalancing.Enums;

namespace Tilework.LoadBalancing.Interfaces;

public interface ILoadBalancerService
{
    public Task<List<LoadBalancerDTO>> GetLoadBalancers();
    public Task<LoadBalancerDTO?> GetLoadBalancer(Guid Id);
    public Task<LoadBalancerDTO> AddLoadBalancer(LoadBalancerDTO balancer);
    public Task<LoadBalancerDTO> UpdateLoadBalancer(LoadBalancerDTO balancer);
    public Task DeleteLoadBalancer(Guid Id);

    public Task EnableLoadBalancer(Guid Id);
    public Task DisableLoadBalancer(Guid Id);

    

    public Task<List<RuleDTO>> GetRules(LoadBalancerDTO balancer);
    public Task AddRule(LoadBalancerDTO balancer, RuleDTO rule);
    public Task UpdateRule(LoadBalancerDTO balancer, RuleDTO rule);
    public Task RemoveRule(LoadBalancerDTO balancer, RuleDTO rule);


    public Task<List<CertificateDTO>> GetCertificates(LoadBalancerDTO balancer);
    public Task AddCertificate(LoadBalancerDTO balancer, Guid certificateId);
    public Task RemoveCertificate(LoadBalancerDTO balancer, Guid certificateId);


    public Task<List<TargetGroupDTO>> GetTargetGroups();

    public Task<TargetGroupDTO?> GetTargetGroup(Guid Id);
    public Task<TargetGroupDTO> AddTargetGroup(TargetGroupDTO group);
    public Task<TargetGroupDTO> UpdateTargetGroup(TargetGroupDTO group);
    public Task DeleteTargetGroup(Guid Id);


    public Task<List<TargetDTO>> GetTargets(TargetGroupDTO group);
    public Task<TargetDTO> GetTarget(Guid Id);
    public Task AddTarget(TargetGroupDTO group, TargetDTO target);
    public Task UpdateTarget(TargetGroupDTO group, TargetDTO target);
    public Task RemoveTarget(TargetGroupDTO group, TargetDTO target);
    public Task<LoadBalancerStatus> GetTargetHealth(Guid id);

    public Task<List<LoadBalancingMonitorData>> GetLoadBalancerMonitoringData(Guid Id, TimeSpan interval, DateTimeOffset start, DateTimeOffset end);

    public Task ApplyConfiguration(Guid Id);
    public Task ApplyConfiguration();
    public Task Shutdown();
}
