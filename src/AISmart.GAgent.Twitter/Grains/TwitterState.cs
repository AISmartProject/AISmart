using System;
using Orleans;

namespace AISmart.Grains;

[GenerateSerializer]
public class TwitterState
{
    [Id(0)] public  Guid Id { get; set; }
}