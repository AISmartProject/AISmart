using AISmart.Agents;

namespace AISmart.GAgent.Autogen;

public interface IAutogenGAgent : IGAgent
{
    void RegisterAgentEvent(Type agent, List<Type> eventTypes);
}