using System;
using System.Text.Json;
using System.Threading;
using AISmart.CQRS.Dto;
using AISmart.CQRS.Options;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
namespace AISmart.CQRS.Service;

public class KafkaConsumerService : ITransientDependency
{
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly ConsumerConfig _consumerConfig;
    private readonly string _topic;
    private readonly IOptionsMonitor<KafkaOptions> _kafkaOptions;


    public KafkaConsumerService(IOptionsMonitor<KafkaOptions> kafkaOptions, ILogger<KafkaConsumerService> logger)
    {
        _logger = logger;
        _consumerConfig = new ConsumerConfig
        {
            BootstrapServers = kafkaOptions.CurrentValue.BootstrapServers,
            GroupId = kafkaOptions.CurrentValue.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };
        _topic = kafkaOptions.CurrentValue.Topic;
    }

    public void StartConsuming(CancellationToken cancellationToken)
    {
        using var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();
        consumer.Subscribe(_topic);
        _logger.LogInformation("StartConsuming...");
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var consumeResult = consumer.Consume(cancellationToken);
                var messageValue = consumeResult.Message.Value;
                var command = JsonSerializer.Deserialize<SaveStateCommand>(messageValue);
                _logger.LogInformation("Received message {message} at: {topicPartitionOffset}.",
                    consumeResult.Message.Value,consumeResult.TopicPartitionOffset);
            }
        }
        catch (OperationCanceledException)
        {
            consumer.Close();
        }
    }
}