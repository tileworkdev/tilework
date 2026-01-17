using System.ComponentModel.DataAnnotations;

namespace Tilework.Ui.Models;

public class EditLoadBalancerForm : BaseForm
{
    [Required]
    public string Name { get; set; }

    [Required]
    public int? Port { get; set; }
}
