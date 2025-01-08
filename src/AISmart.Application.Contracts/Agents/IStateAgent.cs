using System.Threading.Tasks;
using Orleans;

namespace AISmart.Agents;

public interface IAgent: IGrainWithGuidKey
{
    //used for activating the agent manually, mostly used for testing
    Task ActivateAsync();
    
    //probably need a function to get event description
    //Task<string> GetEventDescriptionAsync();
    
    //Function to get agent description
    Task<string> GetDescriptionAsync();
    Task<bool> SubscribeTo(IAgent agent);
    Task<bool> UnsubscribeFrom(IAgent agent);
    Task<bool> PublishTo(IAgent agent);
    Task<bool> UnpublishFrom(IAgent agent);
    Task Register(IAgent agent);
    Task Unregister(IAgent agent);
}

public interface IStateAgent<TState>:  IAgent
{
    Task<TState> GetStateAsync();
}