using System.Text;
using System.Text.Json;

namespace Tilework.Logging.Alloy;

public class ConfigBlock
{
    public string Type { get; }
    public string? Label { get; }
    public List<ConfigStatement> Statements { get; } = new();
    public List<ConfigBlock> Blocks { get; } = new();

    public ConfigBlock(string type, string? label = null)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("A block type is required", nameof(type));

        Type = type;
        Label = label;
    }

    public ConfigBlock AddStatement(string name, ConfigValue value)
    {
        Statements.Add(new ConfigStatement(name, value));
        return this;
    }

    public ConfigBlock Set(string name, string value)
    {
        return AddStatement(name, ConfigValue.String(value));
    }

    public ConfigBlock SetRaw(string name, string value)
    {
        return AddStatement(name, ConfigValue.Raw(value));
    }

    public ConfigBlock SetArray(string name, params ConfigValue[] values)
    {
        return AddStatement(name, ConfigValue.Array(values));
    }

    public ConfigBlock SetMap(string name, IEnumerable<KeyValuePair<string, string>> values)
    {
        return AddStatement(name, ConfigValue.Map(values));
    }

    public ConfigBlock AddBlock(ConfigBlock block)
    {
        Blocks.Add(block ?? throw new ArgumentNullException(nameof(block)));
        return this;
    }

    public ConfigBlock AddBlock(string type, string? label = null)
    {
        var block = new ConfigBlock(type, label);
        Blocks.Add(block);
        return block;
    }

    internal void WriteTo(StringBuilder builder, int indent)
    {
        builder.Append(' ', indent);
        builder.Append(Type);
        if (Label != null)
        {
            builder.Append(' ');
            builder.Append(JsonSerializer.Serialize(Label));
        }
        builder.AppendLine(" {");

        foreach (var statement in Statements)
            statement.WriteTo(builder, indent + 2);

        if (Statements.Count > 0 && Blocks.Count > 0)
            builder.AppendLine();

        for (var index = 0; index < Blocks.Count; index++)
        {
            Blocks[index].WriteTo(builder, indent + 2);
            if (index < Blocks.Count - 1)
                builder.AppendLine();
        }

        builder.Append(' ', indent);
        builder.AppendLine("}");
    }
}
