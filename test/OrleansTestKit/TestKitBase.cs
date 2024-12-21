using AISmart.Agents;
using AISmart.Application.Grains.Agents.Group;
using AISmart.Application.Grains.Agents.Publisher;
using AISmart.Sender;

namespace Orleans.TestKit;

/// <summary>A unit test base class that provides a default mock grain activation context.</summary>
public abstract class TestKitBase
{
    protected TestKitSilo Silo { get; } = new();
}