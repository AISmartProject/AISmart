using AISmart.Agents;

namespace AISmart.GAgent.Core.Context;

public class ContextStorageHelper
{
    private readonly IGrainFactory? _grainFactory;

    public ContextStorageHelper(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
    }

    public async Task<GrainId?> SetContextAsync(EventWrapperBase item, EventBase eventType)
    {
        if (_grainFactory == null)
        {
            return null;
        }

        var contextStorageGrainIdValue = item.GetType()
            .GetProperty(nameof(EventWrapper<EventBase>.ContextStorageGrainId))?
            .GetValue(item);
        GrainId? contextStorageGrainId = null;
        if (contextStorageGrainIdValue == null)
        {
            return contextStorageGrainId;
        }

        contextStorageGrainId = (GrainId)contextStorageGrainIdValue;
        var contextStorageGrain =
            _grainFactory.GetGrain<IContextStorageGrain>(contextStorageGrainId.Value.GetGuidKey());
        var context = await contextStorageGrain.GetContext();
        eventType.SetContext(context);
        return contextStorageGrainId;
    }
}