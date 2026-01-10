using System.ComponentModel.DataAnnotations;


namespace Tilework.Ui.Models;

public class NewUserForm : BaseForm
{
    [Required]
    public string Username { get; set; }
    [Required]
    public string Email { get; set; }
}
