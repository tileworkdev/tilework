using System.Text.Json;

namespace Tilework.Logging.Alloy;

internal sealed class ConfigurationParser
{
    private readonly ConfigurationLexer _lexer;
    private ConfigToken _current;

    public ConfigurationParser(string text)
    {
        _lexer = new ConfigurationLexer(text ?? throw new ArgumentNullException(nameof(text)));
        _current = _lexer.Next();
    }

    public Configuration Parse()
    {
        var configuration = new Configuration();
        while (_current.Kind != ConfigTokenKind.End)
            configuration.AddBlock(ParseBlock());
        return configuration;
    }

    private ConfigBlock ParseBlock()
    {
        var type = Expect(ConfigTokenKind.Identifier).Value;
        string? label = null;
        if (_current.Kind == ConfigTokenKind.String)
            label = Take().Value;

        Expect(ConfigTokenKind.LeftBrace);
        return ParseBlockBody(new ConfigBlock(type, label));
    }

    private ConfigBlock ParseBlockBody(ConfigBlock block)
    {
        while (_current.Kind != ConfigTokenKind.RightBrace)
        {
            if (_current.Kind == ConfigTokenKind.End)
                throw Error("Unexpected end of configuration inside block");

            var name = Expect(ConfigTokenKind.Identifier).Value;
            if (_current.Kind == ConfigTokenKind.Equals)
            {
                Take();
                block.AddStatement(name, ParseValue());
                if (_current.Kind == ConfigTokenKind.Comma)
                    Take();
                continue;
            }

            string? label = null;
            if (_current.Kind == ConfigTokenKind.String)
                label = Take().Value;
            Expect(ConfigTokenKind.LeftBrace);
            block.AddBlock(ParseBlockBody(new ConfigBlock(name, label)));
        }

        Expect(ConfigTokenKind.RightBrace);
        return block;
    }

    private ConfigValue ParseValue()
    {
        if (_current.Kind == ConfigTokenKind.String)
            return ConfigValue.String(Take().Value);
        if (_current.Kind == ConfigTokenKind.Identifier)
            return ConfigValue.Raw(Take().Value);
        if (_current.Kind == ConfigTokenKind.LeftBracket)
            return ParseArray();
        if (_current.Kind == ConfigTokenKind.LeftBrace)
            return ParseMap();

        throw Error($"Expected a value but found {_current.Kind}");
    }

    private ConfigValue ParseArray()
    {
        Expect(ConfigTokenKind.LeftBracket);
        var values = new List<ConfigValue>();
        while (_current.Kind != ConfigTokenKind.RightBracket)
        {
            values.Add(ParseValue());
            if (_current.Kind == ConfigTokenKind.Comma)
                Take();
            else if (_current.Kind != ConfigTokenKind.RightBracket)
                throw Error("Expected ',' or ']' in array");
        }
        Expect(ConfigTokenKind.RightBracket);
        return new ArrayConfigValue(values);
    }

    private ConfigValue ParseMap()
    {
        Expect(ConfigTokenKind.LeftBrace);
        var values = new List<KeyValuePair<string, ConfigValue>>();
        while (_current.Kind != ConfigTokenKind.RightBrace)
        {
            if (_current.Kind is not (ConfigTokenKind.Identifier or ConfigTokenKind.String))
                throw Error("Expected a map key");

            var key = Take().Value;
            Expect(ConfigTokenKind.Equals);
            values.Add(new KeyValuePair<string, ConfigValue>(key, ParseValue()));
            if (_current.Kind == ConfigTokenKind.Comma)
                Take();
            else if (_current.Kind != ConfigTokenKind.RightBrace)
                throw Error("Expected ',' or '}' in map");
        }
        Expect(ConfigTokenKind.RightBrace);
        return new MapConfigValue(values);
    }

    private ConfigToken Expect(ConfigTokenKind kind)
    {
        if (_current.Kind != kind)
            throw Error($"Expected {kind} but found {_current.Kind}");
        return Take();
    }

    private ConfigToken Take()
    {
        var token = _current;
        _current = _lexer.Next();
        return token;
    }

    private FormatException Error(string message) =>
        new($"{message} at position {_current.Position}");
}

internal enum ConfigTokenKind
{
    Identifier,
    String,
    LeftBrace,
    RightBrace,
    LeftBracket,
    RightBracket,
    Equals,
    Comma,
    End
}

internal readonly record struct ConfigToken(ConfigTokenKind Kind, string Value, int Position);

internal sealed class ConfigurationLexer
{
    private readonly string _text;
    private int _position;

    public ConfigurationLexer(string text)
    {
        _text = text;
    }

    public ConfigToken Next()
    {
        SkipTrivia();
        if (_position >= _text.Length)
            return new ConfigToken(ConfigTokenKind.End, string.Empty, _position);

        var position = _position;
        return _text[_position] switch
        {
            '{' => Single(ConfigTokenKind.LeftBrace),
            '}' => Single(ConfigTokenKind.RightBrace),
            '[' => Single(ConfigTokenKind.LeftBracket),
            ']' => Single(ConfigTokenKind.RightBracket),
            '=' => Single(ConfigTokenKind.Equals),
            ',' => Single(ConfigTokenKind.Comma),
            '"' => ReadString(),
            _ => ReadIdentifier(position)
        };
    }

    private ConfigToken Single(ConfigTokenKind kind)
    {
        return new ConfigToken(kind, _text[_position++].ToString(), _position - 1);
    }

    private ConfigToken ReadString()
    {
        var start = _position++;
        var escaped = false;
        while (_position < _text.Length)
        {
            var character = _text[_position++];
            if (escaped)
            {
                escaped = false;
                continue;
            }
            if (character == '\\')
            {
                escaped = true;
                continue;
            }
            if (character == '"')
            {
                var raw = _text[start.._position];
                var value = JsonSerializer.Deserialize<string>(raw)
                    ?? throw new FormatException($"Invalid string at position {start}");
                return new ConfigToken(ConfigTokenKind.String, value, start);
            }
        }

        throw new FormatException($"Unterminated string at position {start}");
    }

    private ConfigToken ReadIdentifier(int start)
    {
        while (_position < _text.Length && !IsDelimiter(_text[_position]))
            _position++;

        if (_position == start)
            throw new FormatException($"Unexpected character '{_text[_position]}' at position {_position}");

        return new ConfigToken(ConfigTokenKind.Identifier, _text[start.._position], start);
    }

    private void SkipTrivia()
    {
        while (_position < _text.Length)
        {
            if (char.IsWhiteSpace(_text[_position]))
            {
                _position++;
                continue;
            }
            if (_text[_position] == '#')
            {
                SkipLine();
                continue;
            }
            if (_text[_position] == '/' && _position + 1 < _text.Length && _text[_position + 1] == '/')
            {
                SkipLine();
                continue;
            }
            break;
        }
    }

    private void SkipLine()
    {
        while (_position < _text.Length && _text[_position] != '\n')
            _position++;
    }

    private static bool IsDelimiter(char character) =>
        char.IsWhiteSpace(character) || character is '{' or '}' or '[' or ']' or '=' or ',' or '"';
}
