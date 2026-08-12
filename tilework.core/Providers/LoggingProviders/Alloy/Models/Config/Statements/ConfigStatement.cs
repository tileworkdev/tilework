using System.Text;

namespace Tilework.Logging.Alloy;

public class ConfigStatement
{
    public string Name { get; }
    public ConfigValue Value { get; }

    public ConfigStatement(string name, ConfigValue value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A statement name is required", nameof(name));

        Name = name;
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    internal void WriteTo(StringBuilder builder, int indent)
    {
        builder.Append(' ', indent);
        builder.Append(Name);
        builder.Append(" = ");
        Value.WriteTo(builder, indent);
        builder.AppendLine();
    }
}
