using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.GAgent.Core;
using AISmart.GAgent.Core.Context;
using AISmart.GAgents.Tests.TestEvents;
using Microsoft.Extensions.Logging.Abstractions;

namespace AISmart.GAgents.Tests.TestGAgents;

[GenerateSerializer]
public class ContextTestGAgentState : StateBase
{
    [Id(0)]  public Dictionary<string, string> SendMessages { get; set; }
    [Id(1)]  public bool Success { get; set; }
}

[GAgent]
public class ContextTestGAgentBase : ContextGAgentBase<ContextTestGAgentState, MessageGEvent>
{
    public ContextTestGAgentBase() : base(NullLogger<ContextTestGAgentBase>.Instance)
    {

    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult("This is a GAgent to test context passing.");
    }
}

[GAgent]
public class ContextTestGAgent1 : ContextTestGAgentBase
{
    public async Task HandleEventAsync(ContextTestEvent1 eventData)
    {
        await this.SetContextAsync("Set context1", "set context1");
        await this.ResetContextStorageGrainTerminateTimeAsync(TimeSpan.FromMinutes(1));
        if (eventData.TryGetContext("Test1", out var testData)
            && testData != null)
        {
            if (eventData.ExpectedContext["Test1"] == testData)
            {
                await PublishAsync(new ContextTestEvent2
                {
                    ExpectedContext = new Dictionary<string, object?>
                    {
                        ["Test1"] = testData,
                        ["Test2"] = 123
                    }
                }.WithContext("Test2", 123));
            }
        }
    }
}


[GAgent]
public class ContextTestGAgent2 : ContextTestGAgentBase
{
    public async Task HandleEventAsync(ContextTestEvent2 eventData)
    {
        await this.SetContextAsync("Set context2", "set context2");
        await this.ResetContextStorageGrainTerminateTimeAsync(TimeSpan.FromMinutes(2));
        if (eventData.TryGetContext("Test1", out var testData)
            && testData != null
            && eventData.TryGetContext("Test2", out var testData2)
            && testData2 != null)
        {
            if (eventData.ExpectedContext["Test1"]!.Equals(testData) && eventData.ExpectedContext["Test2"]!.Equals(testData2))
            {
                await PublishAsync(new ContextTestEvent3
                {
                    ExpectedContext = new Dictionary<string, object?>
                    {
                        ["Test1"] = testData,
                        ["Test2"] = testData2,
                        ["Test3"] = new Dictionary<string, string> { { "testKey", "testValue" } }
                    }
                }.WithContext("Test3", new Dictionary<string, string> { { "testKey", "testValue" } }));
            }
        }
    }
}


[GAgent]
public class ContextTestGAgent3 : ContextTestGAgentBase
{
    public async Task HandleEventAsync(ContextTestEvent3 eventData)
    {
        var getContext = await this.GetContextAsync();
        await this.ResetContextStorageGrainTerminateTimeAsync(TimeSpan.FromMinutes(3));
        if (eventData.TryGetContext("Test1", out var testData)
            && testData != null
            && eventData.TryGetContext("Test2", out var testData2)
            && testData2 != null
            && eventData.TryGetContext("Test3", out var testData3)
            && testData3 != null)
        {
            if (eventData.ExpectedContext["Test1"]!.Equals(testData)
                && eventData.ExpectedContext["Test2"]!.Equals(testData2)
                && ((Dictionary<string, string>)eventData.ExpectedContext["Test3"]!)["testKey"].Equals("testValue"))
            {
                State.Success = true;
                await PublishAsync(new RequestAllSubscriptionsEvent());
            }
        }
    }
}