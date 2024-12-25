namespace AISmart.GAgent.Core.Context;

public interface IEventContext
{
    GrainId? ContextStorageGrainId { get; set; }
    IGrainFactory GrainFactory { get; set; }
    void SetContextStorageGrainId(GrainId? grainId);
    void ClearContext();
}