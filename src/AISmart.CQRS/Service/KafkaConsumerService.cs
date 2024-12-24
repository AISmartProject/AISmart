using System;
using System.Threading;
using System.Threading.Tasks;
using AISmart.CQRS.Dto;
using AISmart.CQRS.Options;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Volo.Abp.DependencyInjection;
namespace AISmart.CQRS.Service;

public class KafkaConsumerService : ITransientDependency
{
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly ConsumerConfig _consumerConfig;
    private readonly string _topic;
    private readonly IOptionsMonitor<KafkaOptions> _kafkaOptions;
    private readonly IIndexingService  _indexingService ;


    public KafkaConsumerService(IOptionsMonitor<KafkaOptions> kafkaOptions, ILogger<KafkaConsumerService> logger, IIndexingService indexingService
    )
    {
         _indexingService = indexingService;
        _logger = logger;
        _consumerConfig = new ConsumerConfig
        {
            BootstrapServers = kafkaOptions.CurrentValue.BootstrapServers,
            GroupId = kafkaOptions.CurrentValue.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };
        _topic = kafkaOptions.CurrentValue.Topic;
    }

    public async Task StartConsuming(CancellationToken cancellationToken)
    {
        using var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();
        consumer.Subscribe(_topic);
        _logger.LogInformation("StartConsuming...");
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var consumeResult = consumer.Consume(cancellationToken);
                try
                {
                    var messageValue = consumeResult.Message.Value;
                    var messageIndex = JsonConvert.DeserializeObject<BaseStateIndex>(messageValue);
                    _logger.LogInformation("Received message {message} at: {topicPartitionOffset}.",
                        consumeResult.Message.Value,consumeResult.TopicPartitionOffset);
                    _indexingService.CheckExistOrCreateIndex(messageIndex.StateType);
                    await SaveIndexAsync(messageIndex.StateType, messageIndex);
                }
                catch (Exception e)
                {
                    consumer.Commit(consumeResult);
                }

               
            }
        }
        catch (OperationCanceledException)
        {
            consumer.Close();
        }
    }
    private async Task SaveIndexAsync(string indexName , BaseStateIndex index)
    {
        await _indexingService.SaveOrUpdateIndexAsync(indexName, index);
    }
}