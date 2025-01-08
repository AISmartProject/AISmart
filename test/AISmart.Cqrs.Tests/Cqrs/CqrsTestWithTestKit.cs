using AISmart.Agent;
using AISmart.Agent.Events;
using AISmart.CQRS;
using AISmart.CQRS.Dto;
using AISmart.CQRS.Handler;
using AISmart.CQRS.Provider;
using AISmart.GAgents.Tests;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using Moq;
using Orleans.TestKit;

namespace AISmart.GAgent;

public class CqrsTestWithTestKit : GAgentTestKitBase
{
   private readonly ITestOutputHelper _output;
    private readonly ICQRSProvider _cqrsProvider;
    private readonly Mock<IIndexingService> _mockIndexingService;
    private const string ChainId = "AELF";
    private const string SenderName = "Test";
    private const string Address = "JRmBduh4nXWi1aXgdUsj5gJrzeZb2LxmrAbf7W99faZSvoAaE";
    private const string IndexName = "aelfagentgstateindex";

    private const string IndexId = "1";

    public CqrsTestWithTestKit(ITestOutputHelper output)
    {
        _output = output;
        _mockIndexingService = new Mock<IIndexingService>();
        _mockIndexingService.Setup(service =>
                service.SaveOrUpdateStateIndexAsync(It.IsAny<string>(), It.IsAny<BaseStateIndex>()))
            .Returns(Task.CompletedTask);
        _mockIndexingService.Setup(b => b.QueryStateIndexAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string id, string indexName) => new BaseStateIndex
                { Id = IndexId.ToString(), Ctime = DateTime.Now, State = Address });

        var services = new ServiceCollection();
        services.AddSingleton<IIndexingService>(_mockIndexingService.Object);
        services.AddMediatR(typeof(SaveStateCommandHandler).Assembly);
        services.AddMediatR(typeof(GetStateQueryHandler).Assembly);
        services.AddMediatR(typeof(SendEventCommandHandler).Assembly);
        services.AddMediatR(typeof(SaveGEventCommandHandler).Assembly);
        services.AddMediatR(typeof(GetGEventQueryHandler).Assembly);
        services.AddMediatR(typeof(SaveLogCommandHandler).Assembly);
        services.AddMediatR(typeof(GetLogQueryHandler).Assembly);

        services.AddSingleton<ICQRSProvider, CQRSProvider>();

        var serviceProvider = services.BuildServiceProvider();
        _cqrsProvider = serviceProvider.GetRequiredService<ICQRSProvider>();
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

        var _clusterClient = await Silo.CreateGrainAsync<AElfGAgent>(guid);
        Silo.AddProbe<IAElfAgent>(_ => _clusterClient);

        await _clusterClient.ExecuteTransactionAsync(createTransactionEvent);
        var transaction = await _clusterClient.GetAElfAgentDto();
        _output.WriteLine("TransactionId: " + transaction.PendingTransactions.Count);
        //get grain
        var grainResult = await _clusterClient.GetAElfAgentDto();
        grainResult.PendingTransactions.Count.ShouldBe(1);
        grainResult.PendingTransactions.FirstOrDefault().Value.ChainId.ShouldBe(createTransactionEvent.ChainId);

        //get cqrs
        var grainId = _clusterClient.GetGrainId();

        var esResult = await _cqrsProvider.QueryAsync(IndexName, grainId.ToString());
        esResult.State.ShouldContain(Address);
        esResult.Id.ShouldBe(IndexId);
    }
}