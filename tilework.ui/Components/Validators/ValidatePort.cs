public static partial class Validators
{
    public static string? ValidatePort(int? value)
    {
        if (value == null || value > 65535 || value < 1)
            return "Value must be between 1 and 65535";
        return null;
    }
}
