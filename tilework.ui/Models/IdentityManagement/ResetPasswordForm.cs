using System.ComponentModel.DataAnnotations;

namespace Tilework.Ui.Models;

public class ResetPasswordForm : BaseForm
{
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "New password")]
    public string NewPassword { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Password confirmation does not match.")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; }
}
