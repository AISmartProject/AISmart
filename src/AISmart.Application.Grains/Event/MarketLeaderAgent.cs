using AISmart.Domain.Grains.Event;
using Volo.Abp.EventBus;
using Volo.Abp.ObjectMapping;

namespace AISmart.Application.Grains.Event;

public interface IMarketLeaderAgent : IAgent,ILocalEventHandler<TelegramEvent>,ILocalEventHandler<MarketOperatoerCompleteEvent>
{
    /// <summary>
    /// Executes a market-leading strategy asynchronously.
    /// </summary>
    /// <param name="eventData">The event to be published.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task ExecuteStrategyAsync(TelegramEvent eventData);
    public Task CompelteStrategyAsync();
}

public class MarketLeaderAgent : Agent, IMarketLeaderAgent
{
    public MarketLeaderAgent(IObjectMapper objectMapper) : base(objectMapper)
    {
    }

    public async Task ExecuteStrategyAsync(TelegramEvent eventData)
    {
        // Additional logic can be added here before executing the strategy
        await HandleEventAsync(eventData);
    }

    public Task CompelteStrategyAsync()
    {
        return Task.CompletedTask;
    }

    public Task HandleEventAsync(TelegramEvent eventData)
    {
        Console.WriteLine($"Event Received: {eventData.Content}");
        return Task.CompletedTask;
    }

    public Task HandleEventAsync(MarketOperatoerCompleteEvent eventData)
    {
        Console.WriteLine($"Event Received: {eventData.Content}");
        CompelteStrategyAsync();
        return Task.CompletedTask;
    }
}