using System.ComponentModel.DataAnnotations;


namespace Tilework.Ui.Models;

public class EditUserForm : BaseForm
{
    [Required]
    public string Username { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
