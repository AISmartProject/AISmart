using AISmart.Agent;

namespace AiSmart.GAgent.TestAgent.ConclusionAgent;

public interface IConclusionGAgent:IMicroAIGAgent
{
    Task SetVoteCount(int voteCount);
}
