using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace AISmart.Agents;

public interface IGAgent : IGrainWithGuidKey
{
    /// <summary>
    /// Used for activating the agent manually, mostly used for testing
    /// </summary>
    /// <returns></returns>
    Task ActivateAsync();

    //probably need a function to get event description
    //Task<string> GetEventDescriptionAsync();

    //Function to get agent description
    Task<string> GetDescriptionAsync();
    Task<bool> SubscribeToAsync(IGAgent gAgent);
    Task<bool> UnsubscribeFromAsync(IGAgent gAgent);
    Task<bool> PublishToAsync(IGAgent gAgent);
    Task<bool> UnpublishFromAsync(IGAgent gAgent);
    Task RegisterAsync(IGAgent gAgent);
    Task UnregisterAsync(IGAgent gAgent);
    Task<List<Type>?> GetAllSubscribedEventsAsync(bool includeBaseHandlers = false);
}

public interface IStateGAgent<TState> : IGAgent
{
    Task<TState> GetStateAsync();
}