using System;
using AISmart.Agents;
using AISmart.GAgent.Autogen.Events;
using Orleans;

namespace AISmart.GAgent.Autogen.Event;

[GenerateSerializer]
public class AutoGenExecutorEvent : AutoGenInternalEventBase
{
    [Id(0)]public Guid TaskId { get; set; }
    [Id(1)] public TaskExecuteStatus ExecuteStatus { get; set; }
    [Id(2)] public string CurrentCallInfo { get; set; }
    [Id(3)] public string EndContent { get; set; }
}

public enum TaskExecuteStatus
{
    Progressing,
    Break,
    Finish,
}