using AISmart.Agents;
using Orleans.Streams;

namespace AISmart.Application.Grains;

public class EventWrapperBaseAsyncObserver : IAsyncObserver<EventWrapperBase>
{
    private readonly Action<EventWrapperBase> _action;

    public EventWrapperBaseAsyncObserver(Action<EventWrapperBase> action)
    {
        _action = action;
    }

    public async Task OnNextAsync(EventWrapperBase item, StreamSequenceToken? token = null)
    {
        _action(item);
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