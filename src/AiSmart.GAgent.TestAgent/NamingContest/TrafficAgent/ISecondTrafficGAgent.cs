namespace AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;

public interface ISecondTrafficGAgent:ITrafficGAgent
{
    Task SetAskJudgeNumber(int judgeNum);
    
    Task SetRoundNumber(int round);
}