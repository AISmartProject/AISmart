using System.Collections.Generic;
using Orleans;
using Orleans.Runtime;

namespace AISmart.Agents;

[GenerateSerializer]
public abstract class EventBase
{
    [Id(999)] private Dictionary<string, object?> _context = new();

    private List<StreamId> _rootStreamIdList = [];

    public void SetRootStreamIdList(List<StreamId> originStreamId)
    {
        _rootStreamIdList = originStreamId;
    }

    public List<StreamId> GetRootStreamIdList()
    {
        return _rootStreamIdList;
    }

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