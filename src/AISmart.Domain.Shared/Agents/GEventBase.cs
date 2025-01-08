using System;
using Orleans;

namespace AISmart.Agents;

[GenerateSerializer] 
public abstract class GEventBase
{
    [Id(0)] public virtual Guid Id { get; set; }
    [Id(1)]public DateTime Ctime { get; set; }

}