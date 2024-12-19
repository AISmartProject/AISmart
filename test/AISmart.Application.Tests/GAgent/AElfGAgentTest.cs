using System;
using System.Threading.Tasks;
using AISmart.Agent;
using AISmart.Agent.Event;
using AISmart.Agent.Events;
using AISmart.Agent.GEvents;
using AISmart.Agent.Grains;
using Orleans;
using Xunit;
using Xunit.Abstractions;

namespace AISmart.GAgent;

public class AElfGAgentTests : AISmartApplicationTestBase
{
    private readonly IClusterClient _clusterClient;
    private readonly ITestOutputHelper _output;
    public AElfGAgentTests(ITestOutputHelper output)
    {
        _clusterClient = GetRequiredService<IClusterClient>();
        _output = output;
    }

    [Fact]
    public async Task SendTransactionTest()
    {
        string chainId = "AELF";
        string senderName = "Test";
        var createTransactionEvent = new CreateTransactionEvent(){
                ChainId = chainId,
                SenderName = senderName, 
                ContractAddress = "JRmBduh4nXWi1aXgdUsj5gJrzeZb2LxmrAbf7W99faZSvoAaE",
                MethodName = "Transfer",
                };
                var guid = Guid.NewGuid();
                await _clusterClient.GetGrain<IAElfAgent>(guid).ExecuteTransactionAsync(createTransactionEvent);
               var transaction = await _clusterClient.GetGrain<IAElfAgent>(guid).GetAElfAgentDto();
                _output.WriteLine("TransactionId: " + transaction.PendingTransactions.Count);
    }
    
    [Fact]
    public async Task GetTransactionResultTest()
    {
        string chainId = "AELF";
        var guid = Guid.NewGuid();
       _= _clusterClient.GetGrain<ITransactionGrain>(guid).LoadAElfTransactionResultAsync(new QueryTransactionDto(){
            ChainId = chainId,
            TransactionId = "TransactionId1"
        });
       _=  _clusterClient.GetGrain<ITransactionGrain>(guid).LoadAElfTransactionResultAsync(new QueryTransactionDto(){
            ChainId = chainId,
            TransactionId = "TransactionId2"
        });
       await Task.Delay(200000);
    }

}