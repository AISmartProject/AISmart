

using AISmart.Agents;
using Orleans.Runtime;
// ReSharper disable once CheckNamespace
using Orleans;

namespace Aevatar.Core.Abstractions;

[GenerateSerializer]
public class AddChildGEvent : GEventBase
{
    [Id(0)] public GrainId Child { get; set; }
}