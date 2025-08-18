using System.ComponentModel.DataAnnotations;

using Tilework.LoadBalancing.Enums;

namespace Tilework.Ui.Models;

public class NewBaseLoadBalancerForm : BaseForm
{
    [Required]
    public string Name { get; set; }

    [Required]
    public int? Port { get; set; }
    
    public LoadBalancerType Type { get; set; }
}

public class NewNetworkLoadBalancerForm : NewBaseLoadBalancerForm
{
    public NewNetworkLoadBalancerForm()
    {
        Type = LoadBalancerType.NETWORK;
    }
    public NlbProtocol Protocol { get; set; }

    [Required]
    [Display(Name = "Target group")]
    public Guid? TargetGroup { get; set; }
}

public class NewApplicationLoadBalancerForm : NewBaseLoadBalancerForm
{
    public NewApplicationLoadBalancerForm()
    {
        Type = LoadBalancerType.APPLICATION;
    }

    public AlbProtocol Protocol { get; set; }
}