namespace AISmart.GAgent.Autogen.DescriptionManager;

[GenerateSerializer]
public class AgentDescriptionManagerState
{
    [Id(0)] public Dictionary<string, AgentDescriptionInfo> AgentDescription =
        new Dictionary<string, AgentDescriptionInfo>();

    [Id(1)] public string AutoGenAgentEventDescription = string.Empty;
}