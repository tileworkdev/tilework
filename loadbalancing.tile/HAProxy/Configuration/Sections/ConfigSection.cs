using System;
using System.Reflection;
using System.Text;
using System.Collections;

namespace Tilework.LoadBalancing.Haproxy;

public abstract class ConfigSection
{
    public string Section { get; set; }
    public string? Name { get; set; }

    private List<string[]> _statements = new List<string[]>();
    public List<string[]> Statements
    {
        get {
            return _statements.Concat(GetStatements()).ToList();
        }
        set {
            _statements = new List<string[]>();
            foreach(var statement in value)
                SetStatement(statement);
        }
    }

    public ConfigSection(string section)
    {
        Section = section;
    }

    private List<string[]> GetStatements()
    {
        var statements = new List<string[]>();

        PropertyInfo[] properties = this.GetType().GetProperties();

        foreach (var property in properties)
        {
            var attribute = property.GetCustomAttribute(typeof(StatementAttribute)) as StatementAttribute;
            if (attribute != null)
            {
                var value = property.GetValue(this);
                if(value != null)
                {
                    if (value is IEnumerable && !(value is string))
                    {
                        var enumerable = (IEnumerable)value;
                        foreach (var item in enumerable)
                        {
                            statements.Add([attribute.Name, PropToString(item)]);
                        }
                    }
                    else
                    {
                        statements.Add([attribute.Name, PropToString(value)]);
                    }
                }
            }
        }

        return statements;
    }

    private string PropToString(object value)
    {
        if (value.GetType().IsEnum)
        {
            return (value as Enum).GetDescription();
        }
        else
        {
            return value.ToString();
        }
    }

    private void SetStatement(string[] statement)
    {
        PropertyInfo[] properties = this.GetType().GetProperties();
        foreach (var property in properties)
        {
            var attribute = property.GetCustomAttribute(typeof(StatementAttribute)) as StatementAttribute;
            if (attribute != null && attribute.Name == statement[0])
            {
                var constructor = property.PropertyType.GetConstructor(new Type[] { typeof(string[]) });
                if (constructor != null)
                {
                    object propertyValue = constructor.Invoke(new object[] { statement });
                    property.SetValue(this, propertyValue);
                }
                else if (property.PropertyType.IsEnum)
                {
                    object enumValue = Enum.Parse(property.PropertyType, statement[1], ignoreCase: true);
                    property.SetValue(this, enumValue);
                }
                else
                {
                    property.SetValue(this, statement[1]);
                }

                return;
            }
        }

        _statements.Add(statement);
    }
}