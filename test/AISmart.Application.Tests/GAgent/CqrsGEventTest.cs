using System;
using System.Linq;
using System.Threading.Tasks;
using AISmart.Agent;
using AISmart.Agent.Events;
using AISmart.CQRS.Provider;
using Orleans;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace AISmart.GAgent;

public class CqrsGEventTests : AISmartApplicationTestBase
{
    private readonly IClusterClient _clusterClient;
    private readonly ITestOutputHelper _output;
    private readonly ICQRSProvider _cqrsProvider;
    private const string ChainId = "AELF";
    private const string SenderName = "Test";
    private const string Address = "JRmBduh4nXWi1aXgdUsj5gJrzeZb2LxmrAbf7W99faZSvoAaE";
    private const string StateIndexName = "aelfagentgstateindex";
    private const string EventIndexName = "createtransactiongeventindex";
    private const string IndexId = "1";

    public CqrsGEventTests(ITestOutputHelper output)
    {
        _output = output;
        _clusterClient = GetRequiredService<IClusterClient>();
        _cqrsProvider = GetRequiredService<ICQRSProvider>();
    }

    [Fact]
    public async Task SendTransactionGEventTest()
    {
        var createTransactionEvent = new CreateTransactionEvent()
        {
            ChainId = ChainId,
            SenderName = SenderName,
            ContractAddress = Address,
            MethodName = "Transfer",
        };
        var guid = Guid.NewGuid();
        await _clusterClient.GetGrain<IAElfAgent>(guid).ExecuteTransactionAsync(createTransactionEvent);
        var transaction = await _clusterClient.GetGrain<IAElfAgent>(guid).GetAElfAgentDto();
        _output.WriteLine("TransactionId: " + transaction.PendingTransactions.Count);
        //get grain
        var grainResult = await _clusterClient.GetGrain<IAElfAgent>(guid).GetAElfAgentDto();
        grainResult.PendingTransactions.Count.ShouldBe(1);
        grainResult.PendingTransactions.FirstOrDefault().Value.ChainId.ShouldBe(createTransactionEvent.ChainId);
        
        //get cqrs state
        var grainId =  _clusterClient.GetGrain<IAElfAgent>(guid).GetGrainId();
        var esResult = await _cqrsProvider.QueryAsync(StateIndexName, grainId.ToString());
        esResult.State.ShouldContain(Address);
        esResult.Id.ShouldContain(guid.ToString().Replace("-",""));
        
        //get cqrs event
        var gEventId = grainResult.PendingTransactions.FirstOrDefault().Key;
        var eventResult = await _cqrsProvider.QueryGEventAsync(EventIndexName, gEventId.ToString());
        eventResult.ShouldContain(ChainId);
        eventResult.ShouldContain(Address);
    }
}