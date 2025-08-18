using Tilework.LoadBalancing.Enums;

namespace Tilework.Ui.Models;

public class NewTargetGroupForm : BaseForm
{
    public string Name { get; set; }
    public TargetGroupProtocol Protocol { get; set; } 
}