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

    private void SetStatement(string[] statement)
    {
        PropertyInfo[] properties = this.GetType().GetProperties();
        foreach (var property in properties)
        {
            var attribute = property.GetCustomAttribute(typeof(StatementAttribute)) as StatementAttribute;
            if (attribute != null && attribute.Name == statement[0])
            {
                if(typeof(IList).IsAssignableFrom(property.PropertyType) == true)
                {
                    var currentValue = (IList) property.GetValue(this);
                    var elementType = property.PropertyType.GetGenericArguments()[0];
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
        else
        {
            return value.ToString();
        }
    }

    private object StringToProp(Type type, string[] value)
    {

        var constructor = type.GetConstructor(new Type[] { typeof(string[]) });
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
        else
        {
            return value[1];
        }
    }
}