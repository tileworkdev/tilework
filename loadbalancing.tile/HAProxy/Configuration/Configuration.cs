using System.IO;

namespace Haproxy.Configuration;

public class Configuration
{
    private GlobalSection Global { get; set; }
    private DefaultsSection Defaults { get; set; }
    private List<BackendSection> Backends { get; set; } = new List<BackendSection>();
    private List<FrontendSection> Frontends { get; set; } = new List<FrontendSection>();

    private readonly string _filename;
    public Configuration(string filename)
    {
        _filename = filename;

        LoadConfiguration();
    }

    private void LoadConfiguration()
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


        var sections = new List<ConfigSection>();
        ConfigSection CurrentSection = null;
        foreach(var line in cleanedLines)
        {
            if(ConfigSection.IsSection(line))
            {
                if(CurrentSection != null)
                    sections.Add(CurrentSection);
                CurrentSection = new ConfigSection(line);
            }
            else
            {
                CurrentSection.AddStatement(line);
            }
        }

        if(CurrentSection != null)
            sections.Add(CurrentSection);

        foreach(var section in sections)
        {
            switch(section.SectionName)
            {
                case "global":
                    Global = new GlobalSection(section);
                    break;
                case "defaults":
                    Defaults = new DefaultsSection(section);
                    break;
                case "frontend":
                    Frontends.Add(new FrontendSection(section));
                    break;
                case "backend":
                    Backends.Add(new BackendSection(section));
                    break;
            }
        }
    }

    private void SaveConfiguration()
    {

    }
}