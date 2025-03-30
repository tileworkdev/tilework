using System.Collections.Concurrent;
using System;
using System.Reflection;

namespace Tilework.Ui.Models;

public class BaseForm
{
    private readonly ConcurrentDictionary<string, List<SelectOptionItem>> _options = new ConcurrentDictionary<string, List<SelectOptionItem>>();

    public List<SelectOptionItem>? GetOptions(string field)
    {
        if (_options.TryGetValue(field, out var value))
            return value;

        return null;
    }

    public void SetOptions(string field, List<SelectOptionItem>? options)
    {
        if (options == null)
            _options.TryRemove(field, out _);
        else
            _options[field] = options;
    }
}