using System;
using Orleans;

namespace AISmart.Agents;

[GenerateSerializer] 
public abstract class GEventBase
{
    [Id(0)] public Guid Id { get; set; }
}