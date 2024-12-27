using System;
using System.Linq;
using System.Threading.Tasks;
using AISmart.Agent;
using AISmart.Agent.Events;
using AISmart.CQRS.Provider;
using AISmart.GAgent.Dto;
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
   // private readonly Mock<IIndexingService> _mockIndexingService;
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
        /*_mockIndexingService = new Mock<IIndexingService>();
        _mockIndexingService.Setup(service => service.SaveOrUpdateIndexAsync(It.IsAny<string>(), It.IsAny<BaseStateIndex>()))
            .Returns(Task.CompletedTask);
        _mockIndexingService.Setup(b => b.QueryIndexAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string id, string indexName) => new BaseStateIndex { Id = IndexId.ToString(), Ctime = DateTime.Now, State = Address});*/

        /*var services = new ServiceCollection();
        //services.AddSingleton<IIndexingService>(_mockIndexingService.Object); 
        services.AddMediatR(typeof(SaveStateCommandHandler).Assembly);
        services.AddMediatR(typeof(GetStateQueryHandler).Assembly);
        services.AddMediatR(typeof(SendEventCommandHandler).Assembly);
        services.AddMediatR(typeof(SaveGEventCommandHandler).Assembly);
        services.AddSingleton<IIndexingService, ElasticIndexingService>();
        services.AddSingleton<IElasticClient>(provider =>
        {
            var settings =new ConnectionSettings(new Uri("http://127.0.0.1:9200"))
                .DefaultIndex("cqrs");
            return new ElasticClient(settings);
        });
        services.AddSingleton<ICQRSProvider,CQRSProvider>();
        services.AddSingleton<IGrainFactory>(_clusterClient);
        var serviceProvider = services.BuildServiceProvider();
        _cqrsProvider = serviceProvider.GetRequiredService<ICQRSProvider>();*/
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
        var eventResult = await _cqrsProvider.QueryGEventAsync<CreateTransactionGEventIndex>(EventIndexName, gEventId.ToString());
        eventResult.ChainId.ShouldBe(ChainId);
        eventResult.ContractAddress.ShouldBe(Address);
    }
}