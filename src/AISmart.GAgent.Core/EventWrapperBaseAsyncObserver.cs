using AISmart.Agents;
using Orleans.Streams;

namespace AISmart.GAgent.Core;

public class EventWrapperBaseAsyncObserver : IAsyncObserver<EventWrapperBase>
{
    private readonly Func<EventWrapperBase, Task> _action;

    public EventWrapperBaseAsyncObserver(Func<EventWrapperBase, Task> action)
    {
        _action = action;
    }

    public async Task OnNextAsync(EventWrapperBase item, StreamSequenceToken? token = null)
    {
        await _action(item);
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