using System.Text;

namespace Tilework.Logging.Alloy;

public class Configuration
{
    public List<ConfigBlock> Blocks { get; } = new();

    public Configuration AddBlock(ConfigBlock block)
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

    public static Configuration Parse(string text)
    {
        return new ConfigurationParser(text).Parse();
    }

    public static Configuration Load(string path)
    {
        return Parse(File.ReadAllText(path));
    }

    public void Save(string path)
    {
        File.WriteAllText(path, ToString());
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        for (var index = 0; index < Blocks.Count; index++)
        {
            Blocks[index].WriteTo(builder, 0);
            if (index < Blocks.Count - 1)
                builder.AppendLine();
        }
        return builder.ToString();
    }
}
