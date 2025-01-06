using AISmart.Agent.Events;
using AISmart.CQRS;
using AISmart.CQRS.Dto;
using AISmart.CQRS.Handler;
using AISmart.CQRS.Provider;
using AISmart.GAgent.Core;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace AISmart.GAgent;

public class CqrsProviderTest : AISmartApplicationTestBase
{
    private readonly IClusterClient _clusterClient;
    private readonly ICQRSProvider _cqrsProvider;
    private readonly Mock<IIndexingService> _mockIndexingService;
    private const string ChainId = "AELF";
    private const string SenderName = "Test";
    private const string Address = "JRmBduh4nXWi1aXgdUsj5gJrzeZb2LxmrAbf7W99faZSvoAaE";
    private const string IndexName = "aelfagentgstateindex";
    private const string IndexId = "1";

    public CqrsProviderTest(ITestOutputHelper output)
    {
        _clusterClient = GetRequiredService<IClusterClient>();
        _mockIndexingService = new Mock<IIndexingService>();
        _mockIndexingService.Setup(service => service.SaveOrUpdateStateIndexAsync(It.IsAny<string>(), It.IsAny<BaseStateIndex>()))
            .Returns(Task.CompletedTask);
        _mockIndexingService.Setup(b => b.QueryStateIndexAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string id, string indexName) => new BaseStateIndex { Id = IndexId.ToString(), Ctime = DateTime.Now, State = Address});

        var services = new ServiceCollection();
        services.AddSingleton<IIndexingService>(_mockIndexingService.Object); 
        services.AddMediatR(typeof(SaveStateCommandHandler).Assembly);
        services.AddMediatR(typeof(GetStateQueryHandler).Assembly);
        services.AddMediatR(typeof(SendEventCommandHandler).Assembly);
        services.AddMediatR(typeof(SaveGEventCommandHandler).Assembly);
        services.AddMediatR(typeof(GetGEventQueryHandler).Assembly);
        services.AddMediatR(typeof(SaveLogCommandHandler).Assembly);
        services.AddMediatR(typeof(GetLogQueryHandler).Assembly);

        services.AddSingleton<IEventDispatcher,CQRSProvider>();
        services.AddSingleton<ICQRSProvider,CQRSProvider>();
        services.AddSingleton<IGrainFactory>(_clusterClient);
        var serviceProvider = services.BuildServiceProvider();
        _cqrsProvider = serviceProvider.GetRequiredService<ICQRSProvider>();
    }
    
    [Fact]
    public async Task SendEventCommandTest()
    {
        var createTransactionEvent = new CreateTransactionEvent()
        {
            ChainId = ChainId,
            SenderName = SenderName,
            ContractAddress = Address,
            MethodName = "Transfer",
        };
        await _cqrsProvider.SendEventCommandAsync(createTransactionEvent);
    }
}