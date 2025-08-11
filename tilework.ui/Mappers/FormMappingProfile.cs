using AutoMapper;

using Tilework.Ui.Models;
using Tilework.LoadBalancing.Persistence.Models;
using Tilework.CertificateManagement.Persistence.Models;

namespace Tilework.Ui.Mappers;

public class FormMappingProfile : Profile
{
    public FormMappingProfile()
    {
        CreateMap<NewTargetGroupForm, TargetGroup>();
        CreateMap<EditTargetGroupForm, TargetGroup>();

        CreateMap<NewCertificateAuthorityForm, CertificateAuthority>();

        CreateMap<NewApplicationLoadBalancerForm, NewNetworkLoadBalancerForm>();
        CreateMap<NewApplicationLoadBalancerForm, ApplicationLoadBalancer>();
        CreateMap<NewNetworkLoadBalancerForm, NetworkLoadBalancer>();

        CreateMap<NewNetworkLoadBalancerForm, NewApplicationLoadBalancerForm>();
        CreateMap<NewApplicationLoadBalancerForm, NewNetworkLoadBalancerForm>();
    }
}
