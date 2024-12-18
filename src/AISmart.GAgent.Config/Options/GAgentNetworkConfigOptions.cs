using System.Collections.Generic;

namespace AISmart.Options;

public class AgentNetworkConfigOptions
{
    public List<Contract> ContractsList { get; set; }
    public List<Group> Groups { get; set; }
}

public class Contract
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public string GrainType { get; set; }
    
    
}

public class Group
{
    public List<string> AgentsList { get; set; }
    public string Name { get; set; }
}

public class Relation
{
    public string From { get; set; }
    public string To { get; set; }
}