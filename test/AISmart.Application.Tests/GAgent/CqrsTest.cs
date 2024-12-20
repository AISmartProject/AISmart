using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AISmart.Agent;
using AISmart.Agent.Events;
using AISmart.Agents;
using AISmart.Agents.Group;
using AISmart.Agents.X.Events;
using AISmart.CQRS;
using AISmart.CQRS.Dto;
using AISmart.CQRS.Handler;
using AISmart.CQRS.Provider;
using AISmart.Sender;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using Moq;
namespace AISmart.GAgent;

public class CqrsTests : AISmartApplicationTestBase
{
    private readonly IClusterClient _clusterClient;
    private readonly ITestOutputHelper _output;
    private readonly ICQRSProvider _cqrsProvider;
    private readonly Mock<IIndexingService> _mockIndexingService;
   // private Mock<IClusterClient> _clusterClientMock;
    private const string ChainId = "AELF";
    private const string SenderName = "Test";
    private const string Address = "JRmBduh4nXWi1aXgdUsj5gJrzeZb2LxmrAbf7W99faZSvoAaE";
    private const string IndexName = "aelfagentgstateindex";
    private const string IndexId = "1";
   // private SendEventCommandHandler _handler;

    public CqrsTests(ITestOutputHelper output)
    {
        _output = output;
        //_clusterClientMock = new Mock<IClusterClient>();

        _clusterClient = GetRequiredService<IClusterClient>();
        _mockIndexingService = new Mock<IIndexingService>();
       // _clusterClientMock = new Mock<IClusterClient>();
        /*
         _groupAgentMock = new Mock<IStateGAgent<GroupAgentState>>();
         _clusterClientMock.Setup(client => client.GetGrain<IPublishingGAgent>(It.IsAny<Guid>()))
            .Returns(_publishingAgentMock.Object);
        _clusterClientMock.Setup(client => client.GetGrain<IStateGAgent<GroupAgentState>>(It.IsAny<Guid>()))
            .Returns(_groupAgentMock.Object);*/
        _mockIndexingService.Setup(service => service.SaveOrUpdateIndexAsync(It.IsAny<string>(), It.IsAny<BaseStateIndex>()))
            .Returns(Task.CompletedTask);
        _mockIndexingService.Setup(b => b.QueryIndexAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string id, string indexName) => new BaseStateIndex { Id = IndexId.ToString(), Ctime = DateTime.Now, State = Address});

        var services = new ServiceCollection();
        services.AddSingleton<IIndexingService>(_mockIndexingService.Object); 
        services.AddMediatR(typeof(SaveStateCommandHandler).Assembly);
        services.AddMediatR(typeof(GetStateQueryHandler).Assembly);
        services.AddMediatR(typeof(SendEventCommandHandler).Assembly);
       // services.AddSingleton<IGrainFactory>();
        services.AddSingleton<ICQRSProvider,CQRSProvider>();
        services.AddSingleton<IGrainFactory>(_clusterClient);
        var serviceProvider = services.BuildServiceProvider();
        _cqrsProvider = serviceProvider.GetRequiredService<ICQRSProvider>();
       // _handler = new SendEventCommandHandler(_clusterClientMock.Object);

    }

    [Fact]
    public async Task SendTransactionTest()
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
        
        //get cqrs
        var grainId =  _clusterClient.GetGrain<IAElfAgent>(guid).GetGrainId();
        var esResult = await _cqrsProvider.QueryAsync(IndexName, grainId.ToString());
        esResult.State.ShouldContain(Address);
        esResult.Id.ShouldBe(IndexId);
    }

    [Fact]
    public async Task SendEventCommandTest()
    {
        var gevent = new XThreadCreatedEvent
        {
            Id = "mock_x_thread_id",
            Content = "mock_x_thread_content"
        };
        await _cqrsProvider.SendEventCommandAsync(gevent);
        var command = new SendEventCommand()
        {
            Event = gevent
        };
        //await _handler.Handle(command, CancellationToken.None);
    }
}