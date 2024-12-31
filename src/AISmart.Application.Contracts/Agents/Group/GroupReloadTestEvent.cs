using System;
using System.ComponentModel;
using Orleans;

namespace AISmart.Agents.Group;

[Description("Developer Base Event.")]
[GenerateSerializer]
public class GroupReloadTestEvent : EventBase
{
    [Id(0)] public Guid GroupManagerGuid { get; set; }
}