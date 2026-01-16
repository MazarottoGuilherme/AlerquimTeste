using System.Text.Json;
using System.Text.Json.Serialization;
using Confluent.Kafka;
using Orders.Application.Interfaces;

namespace Orders.Infrastructure.Messaging;

public class KafkaEventPublisher : IEventPublisher
{
    private readonly IProducer<string, string> _producer;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public KafkaEventPublisher(string bootstrapServers)
    {
        var config = new ProducerConfig { BootstrapServers = bootstrapServers };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync<TEvent>(TEvent @event)
    {
        var topic = typeof(TEvent).Name.ToLower();
        var value = JsonSerializer.Serialize(@event, JsonOptions);
        
        Console.WriteLine($"[KafkaEventPublisher] Publicando evento no tópico '{topic}': {value}");
        
        await _producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = Guid.NewGuid().ToString(),
            Value = value
        });
        
        Console.WriteLine($"[KafkaEventPublisher] Evento publicado com sucesso no tópico '{topic}'");
    }

}