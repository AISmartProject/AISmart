using Microsoft.Extensions.DependencyInjection;
using Orleans.Storage;

namespace AISmart.GAgent.Core;

public sealed class ContextStorageState
{
    public Dictionary<string, object?> Context { get; set; } = new();
}

public interface IContextStorageGrain : IGrainWithGuidKey
{
    Task AddContext(string key, object? value);
    Task AddContext(Dictionary<string, object?> context);
    Task<Dictionary<string, object?>> GetContext();
}

public class ContextStorageGrain : Grain<ContextStorageState>, IContextStorageGrain
{
    private readonly IPersistentState<ContextStorageState> _context;

    private readonly IGrainStorage _grainStorage;

    public ContextStorageGrain([PersistentState("State")] IPersistentState<ContextStorageState> context)
    {
        _context = context;
    }

    public Task AddContext(string key, object? value)
    {
        if (_context.State.Context.IsNullOrEmpty())
        {
            _context.State.Context = [];
        }

        _context.State.Context[key] = value;
        return Task.CompletedTask;
    }

    public Task AddContext(Dictionary<string, object?> context)
    {
        if (_context.State.Context.IsNullOrEmpty())
        {
            _context.State.Context = context;
        }
        else
        {
            foreach (var keyPair in context)
            {
                _context.State.Context[keyPair.Key] = keyPair.Value;
            }
        }

        return Task.CompletedTask;
    }

    public Task<Dictionary<string, object?>> GetContext()
    {
        return Task.FromResult(_context.State.Context);
    }
}