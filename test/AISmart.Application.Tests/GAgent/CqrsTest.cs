using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AISmart.Agent;
using AISmart.Agent.Events;
using AISmart.CQRS;
using AISmart.CQRS.Dto;
using AISmart.CQRS.Handler;
using AISmart.CQRS.Options;
using AISmart.CQRS.Provider;
using AISmart.CQRS.Service;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    private const string ChainId = "AELF";
    private const string SenderName = "Test";
    private const string Address = "JRmBduh4nXWi1aXgdUsj5gJrzeZb2LxmrAbf7W99faZSvoAaE";
    private const string IndexName = "aelfagentgstateindex";
    private const string IndexId = "1";
    private readonly IServiceProvider _serviceProvider;

    public CqrsTests(ITestOutputHelper output)
    {
        _output = output;

        _clusterClient = GetRequiredService<IClusterClient>();
        _mockIndexingService = new Mock<IIndexingService>();
        _mockIndexingService.Setup(service => service.SaveOrUpdateIndexAsync(It.IsAny<string>(), It.IsAny<BaseStateIndex>()))
            .Returns(Task.CompletedTask);
        _mockIndexingService.Setup(b => b.QueryIndexAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string id, string indexName) => new BaseStateIndex { Id = IndexId.ToString(), Ctime = DateTime.UtcNow, State = Address});

        var services = new ServiceCollection();
        services.AddSingleton<IIndexingService>(_mockIndexingService.Object); 
        services.AddMediatR(typeof(SaveStateCommandHandler).Assembly);
        services.AddMediatR(typeof(GetStateQueryHandler).Assembly);
        services.AddMediatR(typeof(SendEventCommandHandler).Assembly);
        services.AddSingleton<ICQRSProvider,CQRSProvider>();
        services.AddSingleton<IGrainFactory>(_clusterClient);
        
        //consumer
        services.AddLogging(builder =>
        {
            builder.AddConsole();  
        });
        services.Configure<KafkaOptions>(options =>
        {
            options.BootstrapServers = "127.0.0.1:9092";
            options.GroupId = "state-consumer-group";
            options.Topic = "state-topic";
        });
        services.AddSingleton<ILogger<KafkaConsumerService>, Logger<KafkaConsumerService>>();
        services.AddTransient<KafkaConsumerService>();
        
        _serviceProvider = services.BuildServiceProvider();
        _cqrsProvider = _serviceProvider.GetRequiredService<ICQRSProvider>();
    }

    [Fact]
    public async Task SendTransactionTest()
    {
        //run consumer
        var consumerService = _serviceProvider.GetRequiredService<KafkaConsumerService>();
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(10));
        var consumerTask = Task.Run(() => consumerService.StartConsuming(cancellationTokenSource.Token), cancellationTokenSource.Token);
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

        var createTransactionEvent = new CreateTransactionEvent()
        {
            ChainId = ChainId,
            SenderName = SenderName,
            ContractAddress = Address,
            MethodName = "Transfer",
        };
        await _cqrsProvider.SendEventCommandAsync(createTransactionEvent);
        var command = new SendEventCommand()
        {
            Event = createTransactionEvent
        };
    }
}