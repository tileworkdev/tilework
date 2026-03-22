using System.Reflection;

namespace Tilework.Core;

public static class AppMetadata
{
    private static readonly Assembly _assembly = typeof(AppMetadata).Assembly;

    public static string Name { get; } =
        _assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product
        ?? _assembly.GetName().Name
        ?? "Application";

    public static string InformationalVersion { get; } =
        _assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? "0.0.0";

    public static string Version { get; } = InformationalVersion.Split('+')[0];

    public static string DisplayVersion => $"v{Version}";
}
