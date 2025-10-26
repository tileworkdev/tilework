using System.Reflection;
using System.Linq;
using System.Collections;

using Tilework.Core.Enums;

namespace Tilework.Monitoring.Collectd;

public abstract class ConfigSection
{
    public string Section { get; set; }
    public string? Name { get; set; }
    public bool QuoteName { get; set; } = true;
    public List<ConfigSection> Children { get; } = new();

    private List<string[]> _statements = new();
    public List<string[]> Statements
    {
        get
        {
            return _statements.Concat(GetStatements()).ToList();
        }
        set
        {
            _statements = new List<string[]>();
            foreach (var statement in value)
                SetStatement(statement);
        }
    }

    protected ConfigSection(string section, bool quoteName = true)
    {
        Section = section;
        QuoteName = quoteName;
    }

    private List<string[]> GetStatements()
    {
        var statements = new List<string[]>();

        PropertyInfo[] properties = GetType().GetProperties();

        foreach (var property in properties)
        {
            var attribute = property.GetCustomAttribute(typeof(StatementAttribute)) as StatementAttribute;
            if (attribute != null)
            {
                var value = property.GetValue(this);
                if (value != null)
                {
                    if (value is IEnumerable && value is not string)
                    {
                        var enumerable = (IEnumerable)value;
                        foreach (var item in enumerable)
                        {
                            statements.Add(new[] { attribute.Name, PropToString(item) });
                        }
                    }
                    else
                    {
                        statements.Add(new[] { attribute.Name, PropToString(value) });
                    }
                }
            }
        }

        return statements;
    }

    private void SetStatement(string[] statement)
    {
        PropertyInfo[] properties = GetType().GetProperties();
        foreach (var property in properties)
        {
            var attribute = property.GetCustomAttribute(typeof(StatementAttribute)) as StatementAttribute;
            if (attribute != null && attribute.Name == statement[0])
            {
                if (typeof(IList).IsAssignableFrom(property.PropertyType))
                {
                    var currentValue = (IList?)property.GetValue(this);
                    var elementType = property.PropertyType.GetGenericArguments()[0];

                    if (currentValue == null)
                    {
                        var listType = typeof(List<>).MakeGenericType(elementType);
                        currentValue = (IList)Activator.CreateInstance(listType);
                    }

                    currentValue.Add(StringToProp(elementType, statement));
                    property.SetValue(this, currentValue);
                }
                else
                {
                    property.SetValue(this, StringToProp(property.PropertyType, statement));
                }

                return;
            }
        }

        _statements.Add(statement);
    }

    private string PropToString(object value)
    {
        if (value.GetType().IsEnum)
        {
            return (value as Enum).GetDescription();
        }
        else if (value is string s)
        {
            var escaped = s.Replace("\"", "\\\"");
            return $"\"{escaped}\"";
        }
        else
        {
            return value.ToString();
        }
    }

    private object StringToProp(Type type, string[] value)
    {
        var constructor = type.GetConstructor(new[] { typeof(string[]) });
        if (constructor != null)
        {
            object propertyValue = constructor.Invoke(new object[] { value });
            return propertyValue;
        }
        else if (type.IsEnum)
        {
            object enumValue = Enum.Parse(type, value[1], ignoreCase: true);
            return enumValue;
        }
        else if (type == typeof(string))
        {
            var token = value[1];
            if (token.Length >= 2 && token.StartsWith("\"") && token.EndsWith("\""))
            {
                token = token.Substring(1, token.Length - 2).Replace("\\\"", "\"");
            }
            return token;
        }
        else
        {
            return value[1];
        }
    }
}
