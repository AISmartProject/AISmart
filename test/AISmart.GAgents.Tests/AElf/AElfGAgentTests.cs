using AISmart.Agent;
using AISmart.Agent.Events;
using AISmart.Agent.Grains;
using AISmart.Application.Grains.Agents.Publisher;
using AISmart.GAgents.Tests;
using AISmart.Sender;
using Orleans.TestKit;
using Shouldly;

namespace AISmart.GAgents.Tests.AElf;

public class AElfGAgentTests : GAgentTestKitBase
{
    [Fact]
    public async Task SendTransactionTest()
    {
        const string chainId = "AELF";
        const string senderName = "Test";
        var createTransactionEvent = new CreateTransactionEvent
        {
            ChainId = chainId,
            SenderName = senderName,
            ContractAddress = "JRmBduh4nXWi1aXgdUsj5gJrzeZb2LxmrAbf7W99faZSvoAaE",
            MethodName = "Transfer",
        };
        var guid = Guid.NewGuid();
        var aelfGAgent = await Silo.CreateGrainAsync<AElfGAgent>(guid);
        var txGrain = await Silo.CreateGrainAsync<TransactionGrain>(guid);
        Silo.AddProbe<ITransactionGrain>(_ => txGrain);
        var publishingAgent = await Silo.CreateGrainAsync<PublishingGAgent>(guid);
        Silo.AddProbe<IPublishingGAgent>(_ => publishingAgent);

        await aelfGAgent.ExecuteTransactionAsync(createTransactionEvent);

        var aelfGAgentState = await aelfGAgent.GetAElfAgentDto();
        aelfGAgentState.PendingTransactions.Count.ShouldBe(1);
    }
}