namespace AISmart.GAgent.Autogen.Common;

[Serializable]
public class AgentResponse<T>
{
    public string AgentName { get; set; }
    public string EventName { get; set; }
    public T Response { get; set; }
}