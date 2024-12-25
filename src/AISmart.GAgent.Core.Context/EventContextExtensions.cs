using AISmart.GAgent.Core.Context;

namespace AISmart.GAgent.Core.Context;

public static class EventContextExtensions
{
    public static async Task SetContextAsync(this IEventContext context, string key, object? value)
    {
        if (context.ContextStorageGrainId != null)
        {
            var contextStorageGrain =
                context.GrainFactory.GetGrain<IContextStorageGrain>(context.ContextStorageGrainId.Value.GetGuidKey());
            await contextStorageGrain.AddContext(key, value);
        }
    }

    public static async Task SetContextAsync(this IEventContext context, Dictionary<string, object?> contextData)
    {
        if (context.ContextStorageGrainId != null)
        {
            var contextStorageGrain =
                context.GrainFactory.GetGrain<IContextStorageGrain>(context.ContextStorageGrainId.Value.GetGuidKey());
            await contextStorageGrain.AddContext(contextData);
        }
    }

    public static async Task ResetContextStorageGrainTerminateTimeAsync(this IEventContext context, TimeSpan timeSpan)
    {
        if (context.ContextStorageGrainId != null)
        {
            var contextStorageGrain =
                context.GrainFactory.GetGrain<IContextStorageGrain>(context.ContextStorageGrainId.Value.GetGuidKey());
            await contextStorageGrain.ResetSelfTerminateTime(timeSpan);
        }
    }

    public static async Task<Dictionary<string, object?>> GetContextAsync(this IEventContext context)
    {
        if (context.ContextStorageGrainId != null)
        {
            var contextStorageGrain =
                context.GrainFactory.GetGrain<IContextStorageGrain>(context.ContextStorageGrainId.Value.GetGuidKey());
            return await contextStorageGrain.GetContext();
        }

        return new Dictionary<string, object?>();
    }

    public static void SetContextStorageGrainId(this IEventContext context, GrainId? grainId)
    {
        context.ContextStorageGrainId = grainId;
    }

    public static void ClearContext(this IEventContext context)
    {
        context.ContextStorageGrainId = null;
    }
}