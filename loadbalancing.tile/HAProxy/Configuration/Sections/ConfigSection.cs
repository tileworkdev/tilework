namespace Haproxy.Configuration;

public class ConfigSection
{
    private static List<string> _sections = new List<string> {
        "global", "defaults", "frontend", "backend",
        "listen", "peers", "mailers", "userlist", "resolvers"
    };

    public static bool IsSection(string[] statement)
    {
        return _sections.Contains(statement[0].ToLower());
    }

    public string SectionName { get; set; }
    public List<string[]> Statements { get; set; } = new List<string[]>();

    public ConfigSection(string[] sectionStatement)
    {
        SectionName = sectionStatement[0];
    }

    public void AddStatement(string[] statement)
    {
        Statements.Add(statement);
    }
}