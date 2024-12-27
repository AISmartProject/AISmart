using System;
using Orleans;

namespace AISmart.Grains;
[GenerateSerializer]
public class NamingContestState
{
    [Id(0)] public string callBackUrl { get; set; }
}