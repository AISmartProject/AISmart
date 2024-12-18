using System;
using Orleans;

namespace AISmart.Agents.AutoGen;

[GenerateSerializer]
public class AutoGenCreatedEvent: EventBase
{
    [Id(0)]public Guid EventId { get; set; }
    
    /// <summary>
    /// user input
    /// </summary>
    [Id(1)]public string Content { get; set; }
}