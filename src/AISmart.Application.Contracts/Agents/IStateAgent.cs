using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Orleans.Streams;

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
    Task RegisterAsync(IGAgent gAgent);
    Task UnregisterAsync(IGAgent gAgent);
    Task<List<Type>?> GetAllSubscribedEventsAsync(bool includeBaseHandlers = false);
}

public interface IStateGAgent<TState> : IGAgent
{
    Task<TState> GetStateAsync();
}