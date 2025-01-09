using AISmart.Agents;
using Orleans.Streams;

namespace AISmart.GAgent.Core;

public class EventWrapperBaseAsyncObserver : IAsyncObserver<EventWrapperBase>
{
    private readonly Action<EventWrapperBase> _action;

    public string MethodName { get; set; }
    public string ParameterTypeName { get; set; }

    public EventWrapperBaseAsyncObserver(Action<EventWrapperBase> action)
    {
        _action = action;
    }

    public Task OnNextAsync(EventWrapperBase item, StreamSequenceToken? token = null)
    {
        _action(item);
        return Task.CompletedTask;
    }

    public Task OnCompletedAsync()
    {
        return Task.CompletedTask;
    }

    public Task OnErrorAsync(Exception ex)
    {
        return Task.CompletedTask;
    }
}