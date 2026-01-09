using System.ComponentModel.DataAnnotations;
namespace Tilework.Ui.Models;

public class EditTargetGroupForm : BaseForm
{
    [Required]
    public string Name { get; set; }
}