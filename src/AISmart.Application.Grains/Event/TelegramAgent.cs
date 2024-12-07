using AISmart.Domain.Grains.Event;
using Volo.Abp.EventBus.Local;
using Volo.Abp.ObjectMapping;

namespace AISmart.Application.Grains.Event;

public interface ITelegramAgent : IAgent
{
    /// <summary>
    /// Sends a chat message asynchronously.
    /// </summary>
    /// <param name="eventData">The event to be published.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task ChatAsync(TelegramEvent eventData);
}


public class TelegramAgent : Agent, ITelegramAgent
{
    public TelegramAgent(ILocalEventBus localEventBus) : base(localEventBus)
    {
    }

    public async Task ChatAsync(TelegramEvent eventData)
    {
        // Additional logic can be added here before publishing
        await PublishAsync(eventData);
    }
    
    
}