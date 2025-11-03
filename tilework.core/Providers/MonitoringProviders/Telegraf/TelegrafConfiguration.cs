using Tomlyn;
using Tomlyn.Model;

namespace Tilework.Monitoring.Telegraf;

public class TelegrafConfiguration
{
    private readonly string _filename;
    private TomlTable _config;


    public TelegrafConfiguration(string filename)
    {
        _filename = filename;
    }

    public void Load()
    {
        var text = File.ReadAllText(_filename);
        _config = Toml.ToModel(text);
    }

    public void Save()
    {
        var text = Toml.FromModel(_config);
        File.WriteAllText(_filename, text);
    }
}
