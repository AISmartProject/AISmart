using System.Collections.Generic;
using Orleans;

namespace AISmart.Agents;

[GenerateSerializer]
public abstract class EventBase
{
    private Dictionary<string, object?> _context = new();

    public void AddContext(string key, object value)
    {
        _context[key] = value;
    }

    public void SetContext(Dictionary<string, object?> context)
    {
        _context = context;
    }

    public bool TryGetContext(string key, out object? context)
    {
        return _context.TryGetValue(key, out context);
    }

    public Dictionary<string, object?> GetContext()
    {
        return _context;
    }

    public EventBase WithContext(Dictionary<string, object?> context)
    {
        foreach (var keyPair in context)
        {
            _context[keyPair.Key] = keyPair.Value;
        }

        return this;
    }
    
    public EventBase WithContext(string key, object? value)
    {
        _context[key] = value;
        return this;
    }
}