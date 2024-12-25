using AISmart.Agents;
using AISmart.GAgent.Core.Context;
using Microsoft.Extensions.Logging;

namespace AISmart.GAgent.Core;

[GAgent]
public abstract class ContextGAgentBase<TState, TEvent> : GAgentBase<TState, TEvent>, IEventContext
    where TState : StateBase, new()
    where TEvent : GEventBase
{
    public ContextGAgentBase(ILogger logger) : base(logger)
    {
    }

    public abstract override Task<string> GetDescriptionAsync();

    public GrainId? ContextStorageGrainId { get; set; }
    public IGrainFactory GrainFactory { get; set; }

    public void SetContextStorageGrainId(GrainId? grainId)
    {
        ContextStorageGrainId = grainId;
    }

    public void ClearContext()
    {
        ContextStorageGrainId = null;
    }
}