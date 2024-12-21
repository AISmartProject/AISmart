using System.Collections.ObjectModel;

namespace AISmart.GAgent.Autogen.DescriptionManager;

public interface IAgentDescriptionManager:IGrainWithStringKey
{
    Task AddAgentEventsAsync(Type agentType, List<Type> eventTypes);

    Task AddAgentEventsAsync(string agentName, string agentDescription, List<Type> types);
    
    Task<string> GetAutoGenEventDescriptionAsync();
    Task<ReadOnlyDictionary<string, AgentDescriptionInfo>> GetAgentDescription();
}