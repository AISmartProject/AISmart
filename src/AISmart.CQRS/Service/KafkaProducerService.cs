using System;
using System.Text.Json;
using System.Threading.Tasks;
using AISmart.CQRS.Dto;
using AISmart.CQRS.Options;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Volo.Abp.DependencyInjection;
namespace AISmart.CQRS.Service;

public class KafkaProducerService: ITransientDependency
{
    private readonly ILogger<KafkaProducerService> _logger;
    private readonly ProducerConfig _producerConfig;
    private readonly string _topic;
    private readonly IOptionsMonitor<KafkaOptions> _kafkaOptions;

    public KafkaProducerService(IOptionsMonitor<KafkaOptions> kafkaOptions, ILogger<KafkaProducerService> logger)
    {
        _kafkaOptions = kafkaOptions;
        _logger = logger;
        _producerConfig = new ProducerConfig { BootstrapServers = kafkaOptions.CurrentValue.BootstrapServers };
        _topic = kafkaOptions.CurrentValue.Topic;
    }

    public async Task SendAsync(SaveStateCommand command)
    {
        using var producer = new ProducerBuilder<Null, string>(_producerConfig).Build();
        try
        {
            var messageValue = JsonConvert.SerializeObject(command);
            var deliveryResult = await producer.ProduceAsync(_topic, new Message<Null, string> { Value = messageValue });
            _logger.LogInformation("Delivered {result} to {topicPartitionOffset}", deliveryResult.Value, deliveryResult.TopicPartitionOffset);
        }
        catch (Exception ex)
        {
            _logger.LogError("Delivery failed: {error}", ex.Message);
        }
    }
}