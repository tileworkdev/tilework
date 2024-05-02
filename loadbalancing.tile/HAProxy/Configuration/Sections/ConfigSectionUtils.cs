namespace Tilework.LoadBalancing.Haproxy;

public static class ConfigSectionUtils
{
    private static List<string> _sections = new List<string> {
        "global", "defaults", "frontend", "backend",
        "listen", "peers", "mailers", "userlist", "resolvers"
    };

    public static bool IsSection(string[] statement)
    {
        return _sections.Contains(statement[0].ToLower());
    }
}