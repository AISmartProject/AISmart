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

public class CqrsTests : AISmartApplicationTestBase
{
    private readonly IClusterClient _clusterClient;
    private readonly ITestOutputHelper _output;
    private readonly ICQRSProvider _cqrsProvider;

    public CqrsTests(ITestOutputHelper output)
    {
        _clusterClient = GetRequiredService<IClusterClient>();
        _output = output;
        _cqrsProvider = GetRequiredService<ICQRSProvider>();
    }

    [Fact]
    public async Task SendTransactionTest()
    {
        string chainId = "AELF";
        string senderName = "Test";
        string address = "JRmBduh4nXWi1aXgdUsj5gJrzeZb2LxmrAbf7W99faZSvoAaE";
        var createTransactionEvent = new CreateTransactionEvent()
        {
            ChainId = chainId,
            SenderName = senderName,
            ContractAddress = address,
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
        
        //get cqrs
        var grainId =  _clusterClient.GetGrain<IAElfAgent>(guid).GetGrainId();
        var esResult = await _cqrsProvider.QueryAsync("aelfagentgstateindex", grainId.ToString());
        Assert.Contains(address, esResult.State);
    }
}