using AISmart.Agents.AutoGen;
using AISmart.GAgent.Autogen.Common;
using AISmart.GAgent.Autogen.EventSourcingEvent;
using AutoGen.Core;
using Microsoft.Extensions.Logging;

namespace AISmart.GAgent.Autogen.State;

[GenerateSerializer]
public class AutoGenAgentState
{   
    [Id(0)] public Dictionary<Guid, AutoGenAgentStateInfo> AutoGenStateDic =
        new Dictionary<Guid, AutoGenAgentStateInfo>();

    [Id(1)] public Dictionary<Guid, RequestWrapper> CurrentRequestDic = new Dictionary<Guid, RequestWrapper>();
    
    public void Apply(Break @event)
    {
        var state = GetStateInfo(@event.Id);
        if (state == null)
        {
            return;
        }

        state.SessionStateEnum = SessionStateEnum.Break;
        state.BreakReason = @event.BreakReason;

        AutoGenStateDic.Remove(@event.Id);
    }
    
    public void Apply(CallerAgentReply @event)
    {
        var state = GetStateInfo(@event.Id);
        if (state == null)
        {
            return;
        }
        state.ChatHistory.Add(@event.Reply);
    }
    
    public void Apply(CallerProgressing @event)
    {
        var state = GetStateInfo(@event.Id);
        if (state == null)
        {
            return;
        }

        state.CurrentCallInfo = @event.CurrentCallInfo;
    }

    public void Apply(Complete @event)
    {
        var state = GetStateInfo(@event.Id);
        if (state == null)
        {
            return;
        }

        state.SessionStateEnum = SessionStateEnum.Completed;
        state.Summary = @event.Summary;

        AutoGenStateDic.Remove(@event.Id);
    }

    public void Apply(Create @event)
    {
        var state = new AutoGenAgentStateInfo();
        state.TaskId = @event.Id;
        state.ChatHistory = @event.Messages;
        state.SessionStartTime = @event.CreateTime;
        
        AutoGenStateDic.Add(@event.Id, state);
    }

    private AutoGenAgentStateInfo? GetStateInfo(Guid taskId)
    {
        AutoGenStateDic.TryGetValue(taskId, out var result);
        return result;
    }
}

[GenerateSerializer]
public class AutoGenAgentStateInfo
{
    [Id(0)] public Guid TaskId { get; set; }
    [Id(1)] public List<AutogenMessage> ChatHistory { get; set; }
    [Id(2)] public long SessionStartTime { get; set; }
    [Id(3)] public SessionStateEnum SessionStateEnum { get; set; } = SessionStateEnum.Processing;
    [Id(4)] public string CurrentCallInfo { get; set; }
    [Id(5)] public string Summary { get; set; }
    [Id(6)] public string BreakReason { get; set; }
}

[GenerateSerializer]
public enum SessionStateEnum
{
    Processing,
    Break,
    Completed
}

[GenerateSerializer]
public class RequestWrapper
{
    [Id(0)] public Guid TaskId { get; set; }
    [Id(1)] public long StartTime { get; set; }
}
