using System.ComponentModel.DataAnnotations;
using Tilework.Core.Utils;

namespace Tilework.Ui.Components.Validators;

public sealed class HostnameAttribute : ValidationAttribute
{
    public HostnameAttribute() : base("Value must be a valid hostname") { }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string s || string.IsNullOrWhiteSpace(s))
            return ValidationResult.Success;

        try
        {
            var normalized = HostnameUtils.NormalizeHost(s);
            return HostnameUtils.IsValidHostname(normalized)
                ? ValidationResult.Success
                : new ValidationResult(ErrorMessageString);
        }
        catch (ArgumentException)
        {
            return new ValidationResult(ErrorMessageString);
        }
    }
}
