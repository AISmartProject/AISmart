using Orleans;

namespace AISmart.Agents;

public interface IEventWithResponse<TResponse> { }

[GenerateSerializer] 
public abstract class EventBase
{
}