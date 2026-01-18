using System.ComponentModel.DataAnnotations;

using Tilework.LoadBalancing.Enums;

namespace Tilework.Ui.Models;

public class NewLoadBalancerForm : BaseForm
{
    [Required]
    public string Name { get; set; }

    [Required]
    public int? Port { get; set; }
    
    public LoadBalancerType Type { get; set; }
    public LoadBalancerProtocol Protocol { get; set; }
}
