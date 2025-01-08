

using AISmart.Agents;
using Orleans.Runtime;
// ReSharper disable once CheckNamespace
using Orleans;

namespace Aevatar.Core.Abstractions;

[GenerateSerializer]
public class AddSubscriberGEvent : GEventBase
{
    [Id(0)] public GrainId Subscriber { get; set; }
}