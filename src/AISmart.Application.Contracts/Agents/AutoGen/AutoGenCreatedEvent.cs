using System;

namespace AISmart.Agents.AutoGen;

public class AutoGenCreatedEvent: GEvent
{
    public Guid EventId { get; set; }
    /// <summary>
    /// user input
    /// </summary>
    public string Content { get; set; }
}