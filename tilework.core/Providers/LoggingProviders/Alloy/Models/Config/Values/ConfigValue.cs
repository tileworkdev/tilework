using System.Text;
using System.Text.Json;

namespace Tilework.Logging.Alloy;

public abstract class ConfigValue
{
    internal abstract void WriteTo(StringBuilder builder, int indent);

    public static ConfigValue String(string value) => new StringConfigValue(value);
    public static ConfigValue Raw(string value) => new RawConfigValue(value);
    public static ConfigValue Array(params ConfigValue[] values) => new ArrayConfigValue(values);
    public static ConfigValue Map(IEnumerable<KeyValuePair<string, string>> values) =>
        new MapConfigValue(values.Select(value =>
            new KeyValuePair<string, ConfigValue>(value.Key, String(value.Value))));
}

public sealed class StringConfigValue(string value) : ConfigValue
{
    public string Value { get; } = value;

    internal override void WriteTo(StringBuilder builder, int indent)
    {
        builder.Append(JsonSerializer.Serialize(Value));
    }
}

public sealed class RawConfigValue(string value) : ConfigValue
{
    public string Value { get; } = value;

    internal override void WriteTo(StringBuilder builder, int indent)
    {
        builder.Append(Value);
    }
}

public sealed class ArrayConfigValue(IEnumerable<ConfigValue> values) : ConfigValue
{
    public List<ConfigValue> Values { get; } = values.ToList();

    internal override void WriteTo(StringBuilder builder, int indent)
    {
        builder.Append('[');
        for (var index = 0; index < Values.Count; index++)
        {
            if (index > 0)
                builder.Append(", ");
            Values[index].WriteTo(builder, indent);
        }
        builder.Append(']');
    }
}

public sealed class MapConfigValue(IEnumerable<KeyValuePair<string, ConfigValue>> values) : ConfigValue
{
    public List<KeyValuePair<string, ConfigValue>> Values { get; } = values.ToList();

    internal override void WriteTo(StringBuilder builder, int indent)
    {
        if (Values.Count == 0)
        {
            builder.Append("{}");
            return;
        }

        builder.AppendLine("{");
        foreach (var value in Values)
        {
            builder.Append(' ', indent + 2);
            builder.Append(value.Key);
            builder.Append(" = ");
            value.Value.WriteTo(builder, indent + 2);
            builder.AppendLine(",");
        }
        builder.Append(' ', indent);
        builder.Append('}');
    }
}
