using Tilework.LoadBalancing.Models;
using Tilework.CertificateManagement.Models;

namespace Tilework.LoadBalancing.Interfaces;

public interface ILoadBalancerService
{
    public Task<List<BaseLoadBalancerDTO>> GetLoadBalancers();
    public Task<BaseLoadBalancerDTO?> GetLoadBalancer(Guid Id);
    public Task<BaseLoadBalancerDTO> AddLoadBalancer(BaseLoadBalancerDTO balancer);
    public Task<BaseLoadBalancerDTO> UpdateLoadBalancer(BaseLoadBalancerDTO balancer);
    public Task DeleteLoadBalancer(Guid Id);

    public Task EnableLoadBalancer(Guid Id);
    public Task DisableLoadBalancer(Guid Id);

    public Task<List<RuleDTO>> GetRules(ApplicationLoadBalancerDTO balancer);
    public Task AddRule(ApplicationLoadBalancerDTO balancer, RuleDTO rule);
    public Task UpdateRule(ApplicationLoadBalancerDTO balancer, RuleDTO rule);
    public Task RemoveRule(ApplicationLoadBalancerDTO balancer, RuleDTO rule);


    public Task<List<CertificateDTO>> GetCertificates(BaseLoadBalancerDTO balancer);
    public Task AddCertificate(BaseLoadBalancerDTO balancer, Guid certificateId);
    public Task RemoveCertificate(BaseLoadBalancerDTO balancer, Guid certificateId);


    public Task<List<TargetGroupDTO>> GetTargetGroups();
    public Task<List<TargetGroupDTO>> GetNlbTargetGroups();
    public Task<List<TargetGroupDTO>> GetAlbTargetGroups();

    public Task<TargetGroupDTO?> GetTargetGroup(Guid Id);
    public Task<TargetGroupDTO> AddTargetGroup(TargetGroupDTO group);
    public Task<TargetGroupDTO> UpdateTargetGroup(TargetGroupDTO group);
    public Task DeleteTargetGroup(Guid Id);


    public Task<List<TargetDTO>> GetTargets(TargetGroupDTO group);
    public Task AddTarget(TargetGroupDTO group, TargetDTO target);
    public Task UpdateTarget(TargetGroupDTO group, TargetDTO target);
    public Task RemoveTarget(TargetGroupDTO group, TargetDTO target);

    public Task ApplyConfiguration();
    public Task Shutdown();
}