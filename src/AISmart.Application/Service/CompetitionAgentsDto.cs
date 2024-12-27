namespace AISmart.Service;

using System.Collections.Generic;

public class ContestantAgent
{
    public string Name { get; set; }
    public string Label { get; set; }
    public string Bio { get; set; }
    public List<Goal> Goals { get; set; }
}

public class JudgeAgent
{
    public string Name { get; set; }
    public string Label { get; set; }
    public string Bio { get; set; }
    public List<Goal> Goals { get; set; }
}

public class HostAgent
{
    public string Name { get; set; }
    public string Label { get; set; }
    public string Bio { get; set; }
    public List<Goal> Goals { get; set; }
}

public class Goal
{
    public string Action { get; set; }
    public string Description { get; set; }
}

public class CompetitionAgentsDto
{
    public List<ContestantAgent> ContestantAgentList { get; set; }
    public List<JudgeAgent> JudgeAgentList { get; set; }
    public List<HostAgent> HostAgentList { get; set; }
}


public class Network
{
    public List<string> ConstentList { get; set; }
    public List<string> JudgeList { get; set; }
    public List<string> ScoreList { get; set; }
    public List<string> HostList { get; set; }
    public string CallbackAddress { get; set; }
    public string Name { get; set; }
}

public class NetworksDto
{
    public List<Network> Networks { get; set; }
}


public class AgentReponse
{
    public string Name { get; set; }
    public string AgentId { get; set; }
}

public class AgentResponse
{
    public List<AgentReponse> ContestantAgentList { get; set; }
    public List<AgentReponse> JudgeAgentList { get; set; }
    public List<AgentReponse> HostAgentList { get; set; }

    public AgentResponse()
    {
        ContestantAgentList = new List<AgentReponse>();
        JudgeAgentList = new List<AgentReponse>();
        HostAgentList = new List<AgentReponse>();
    }
}


public class GroupDetail
{
    public string GroupId { get; set; }
    public string Name { get; set; }
}

public class GroupResponse
{
    public List<GroupDetail> GroupDetails { get; set; }
}