using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Tilework.Monitoring.Collectd;

public class CollectdConfiguration
{
    public RootSection Root { get; set; } = new RootSection();
    public List<PluginSection> Plugins { get; set; } = new();

    private readonly string _filename;

    public CollectdConfiguration(string filename)
    {
        _filename = filename;
    }

    public void Load()
    {
        var lines = File.ReadAllLines(_filename);

        var cleanedLines = lines.Select(line =>
        {
            int hashIndex = line.IndexOf('#');
            return hashIndex != -1 ? line.Substring(0, hashIndex) : line;
        })
        .Where(line => !string.IsNullOrWhiteSpace(line))
        .ToArray();

        var rootStatements = new List<string[]>();
        var sectionStack = new Stack<(ConfigSection section, List<string[]> statements)>();

        foreach (var rawLine in cleanedLines)
        {
            var line = rawLine.Trim();
            if (line.StartsWith("<") && line.EndsWith(">"))
            {
                var inner = line.Substring(1, line.Length - 2).Trim();
                if (inner.StartsWith("/"))
                {
                    if (sectionStack.Count > 0)
                    {
                        var (closedSection, statements) = sectionStack.Pop();
                        closedSection.Statements = statements;

                        if (sectionStack.Count > 0)
                        {
                            // Attach to parent (generic children collection)
                            var parent = sectionStack.Peek().section;
                            parent.Children.Add(closedSection);

                            // Attach to a typed parent if applicable
                            if (parent is PluginSection plugin && closedSection is ModuleSection module)
                            {
                                plugin.Modules.Add(module);
                            }
                        }
                        else
                        {
                            // Only keep top-level Plugin sections
                            if (closedSection is PluginSection plugin)
                                Plugins.Add(plugin);
                            // Ignore other top-level sections for now
                        }
                    }
                }
                else
                {
                    var parts = inner.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    var sectionName = parts[0];
                    var name = parts.Length > 1 ? parts[1] : null;
                    var newSection = CreateSection(sectionName, name);
                    sectionStack.Push((newSection, new List<string[]>()));
                }
            }
            else
            {
                var statement = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (sectionStack.Count > 0)
                {
                    sectionStack.Peek().statements.Add(statement);
                }
                else
                {
                    rootStatements.Add(statement);
                }
            }
        }

        Root.Statements = rootStatements;
    }

    private static string? Unquote(string? s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        if (s!.Length >= 2 && s.StartsWith("\"") && s.EndsWith("\""))
            return s.Substring(1, s.Length - 2).Replace("\\\"", "\"");
        return s;
    }

    private ConfigSection CreateSection(string section, string? name)
    {
        switch (section)
        {
            case "Plugin":
                return new PluginSection() { Name = Unquote(name) };
            case "Module":
                return new ModuleSection() { Name = Unquote(name) };
            default:
                return new GenericSection(section) { Name = Unquote(name) };
        }
    }

    public void Save()
    {
        var configLines = new List<string>();

        foreach (var statement in Root.Statements)
            configLines.Add(string.Join(" ", statement));

        foreach (var plugin in Plugins)
            WriteSection(configLines, plugin, indentLevel: 0);

        File.WriteAllLines(_filename, configLines);
    }

    private void WriteSection(List<string> lines, ConfigSection section, int indentLevel)
    {
        string indent = new string('\t', indentLevel);

        if (section.Name != null)
        {
            if(!section.QuoteName)
                lines.Add($"{indent}<{section.Section} {section.Name}>");
            else
                lines.Add($"{indent}<{section.Section} \"{section.Name}\">");
        }
        else
            lines.Add($"{indent}<{section.Section}>");

        foreach (var statement in section.Statements)
            lines.Add($"{indent}\t{string.Join(" ", statement)}");

        // Write children (generic + typed, without duplicates)
        var written = new HashSet<ConfigSection>();

        void writeChild(ConfigSection child)
        {
            if (written.Add(child))
                WriteSection(lines, child, indentLevel + 1);
        }

        foreach (var child in section.Children)
            writeChild(child);

        if (section is PluginSection plugin)
        {
            foreach (var module in plugin.Modules)
                writeChild(module);
        }

        lines.Add($"{indent}</{section.Section}>");
    }
}
