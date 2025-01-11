using Orleans;

namespace AISmart.Agents.Messaging;

[GenerateSerializer]
public class SendEvent : EventBase
{
    [Id(0)] public string Message { get; set; } = string.Empty;
}