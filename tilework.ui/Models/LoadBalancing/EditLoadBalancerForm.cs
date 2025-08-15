using System.ComponentModel.DataAnnotations;

using Tilework.Core.LoadBalancing.Enums;

namespace Tilework.Ui.Models;

public class EditBaseLoadBalancerForm : BaseForm
{
    [Required]
    public string Name { get; set; }

    [Required]
    public int? Port { get; set; }
    
    public LoadBalancerType Type { get; set; }
}

public class EditNetworkLoadBalancerForm : EditBaseLoadBalancerForm
{
    public EditNetworkLoadBalancerForm()
    {
        Type = LoadBalancerType.NETWORK;
    }
    public NlbProtocol Protocol { get; set; }
    public Guid TargetGroup { get; set; }
}

public class EditApplicationLoadBalancerForm : EditBaseLoadBalancerForm
{
    public EditApplicationLoadBalancerForm()
    {
        Type = LoadBalancerType.APPLICATION;
    }

    public AlbProtocol Protocol { get; set; }
}