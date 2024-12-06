using AISmart.Domain.Grains.Event;
using Volo.Abp.EventBus;
using Volo.Abp.ObjectMapping;

namespace AISmart.Application.Grains.Event;

public interface IMarketPriceAgent : IAgent,ILocalEventHandler<MarketLeaderCreatedEvent>
{
    /// <summary>
    /// Executes a market-leading strategy asynchronously.
    /// </summary>
    /// <param name="eventData">The event to be published.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task AnalysePriceAsync(MarketLeaderCreatedEvent eventData);
}

public class MarketPriceAgent : Agent, IMarketPriceAgent
{
    public MarketPriceAgent(IObjectMapper objectMapper) : base(objectMapper)
    {
    }

    public async Task AnalysePriceAsync(MarketLeaderCreatedEvent eventData)
    {
        // Additional logic can be added here before executing the strategy
        await HandleEventAsync(eventData);
    }
    
    public Task HandleEventAsync(MarketLeaderCreatedEvent eventData)
    {
        Console.WriteLine($"Event Received: {eventData.Content}");
        return Task.CompletedTask;
    }
    
}