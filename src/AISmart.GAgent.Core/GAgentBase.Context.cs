namespace AISmart.GAgent.Core;

public abstract partial class GAgentBase<TState, TEvent>
{
    private GrainId? _contextStorageGrainId;

    protected async Task SetContextAsync(string key, object? value)
    {
        if (_contextStorageGrainId != null)
        {
            var contextStorageGrain = GrainFactory.GetGrain<IContextStorageGrain>(_contextStorageGrainId.Value.GetGuidKey());
            await contextStorageGrain.AddContext(key, value);
        }
    }
    
    protected async Task SetContextAsync(Dictionary<string, object?> context)
    {
        if (_contextStorageGrainId != null)
        {
            var contextStorageGrain = GrainFactory.GetGrain<IContextStorageGrain>(_contextStorageGrainId.Value.GetGuidKey());
            await contextStorageGrain.AddContext(context);
        }
    }
    
    protected async Task ResetContextStorageGrainTerminateTimeAsync(TimeSpan timeSpan)
    {
        if (_contextStorageGrainId != null)
        {
            var contextStorageGrain = GrainFactory.GetGrain<IContextStorageGrain>(_contextStorageGrainId.Value.GetGuidKey());
            await contextStorageGrain.ResetSelfTerminateTime(timeSpan);
        }
    }

    protected async Task<Dictionary<string, object?>> GetContextAsync()
    {
        if (_contextStorageGrainId != null)
        {
            var contextStorageGrain =
                GrainFactory.GetGrain<IContextStorageGrain>(_contextStorageGrainId.Value.GetGuidKey());
            return await contextStorageGrain.GetContext();
        }

        return new Dictionary<string, object?>();
    }

    private void SetContextStorageGrainId(GrainId? grainId)
    {
        _contextStorageGrainId = grainId;
    }

    private GrainId? GetContextStorageGrainId()
    {
        return _contextStorageGrainId;
    }

    private void ClearContext()
    {
        _contextStorageGrainId = null;
    }
}