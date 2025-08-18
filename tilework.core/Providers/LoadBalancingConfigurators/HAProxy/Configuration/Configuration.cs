using System.IO;

namespace Tilework.LoadBalancing.Haproxy;

public class Configuration
{
    public ConfigSection Global { get; set; }
    public ConfigSection Defaults { get; set; }
    public List<ConfigSection> Backends { get; set; } = new List<ConfigSection>();
    public List<ConfigSection> Frontends { get; set; } = new List<ConfigSection>();

    private readonly string _filename;

    public Configuration(string filename)
    {
        _filename = filename;
    }

    private void LoadSection(string section, string? name, List<string[]> statements)
    {
        switch(section)
        {
            case "global":
                Global = new GlobalSection() { Statements = statements, Name = name };
                break;
            case "defaults":
                Defaults = new DefaultsSection() { Statements = statements, Name = name };
                break;
            case "frontend":
                Frontends.Add(new FrontendSection() { Statements = statements, Name = name });
                break;
            case "backend":
                Backends.Add(new BackendSection() { Statements = statements, Name = name });
                break;
        }
    }

    public void Load()
    {
        // Read the lines
        var lines = File.ReadAllLines(_filename);
        
        var cleanedLines = lines.Select(line => {
                int hashIndex = line.IndexOf('#');
                return hashIndex != -1 ? line.Substring(0, hashIndex) : line;
            })
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
            .ToArray();

        string[] curName = null;
        List<string[]> curStatements = null;
        foreach(var line in cleanedLines)
        {
            if(ConfigSectionUtils.IsSection(line))
            {
                if(curName != null)
                    LoadSection(curName[0], curName.Length > 1 ? curName[1] : null, curStatements);
                curName = line;
                curStatements = new List<string []>();
            }
            else
            {
                curStatements.Add(line);
            }
        }

        if(curName != null)
            LoadSection(curName[0], curName.Length > 1 ? curName[1] : null, curStatements);
    }

    public void Save()
    {
        var configLines = new List<string>();

        configLines.AddRange(generateSectionLines(Global));
        configLines.AddRange(generateSectionLines(Defaults));

        foreach(var fe in Frontends)
            configLines.AddRange(generateSectionLines(fe));
        foreach(var be in Backends)
            configLines.AddRange(generateSectionLines(be));

        File.WriteAllLines(_filename, configLines);
    }

    private List<string> generateSectionLines(ConfigSection section)
    {
        var sectionLines = new List<string>();
        if(section.Name != null)
            sectionLines.Add($"{section.Section} {section.Name}");
        else
            sectionLines.Add(section.Section);

        foreach(var statement in section.Statements)
        {
            sectionLines.Add($"\t{String.Join(" ", statement)}");
        }

        return sectionLines;
    }
}