using System.Text.Json;
using System.Text.Json.Serialization;
using Catalog.Application.Interfaces;
using Confluent.Kafka;

namespace Catalog.Infrastructure.Messaging;

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

        var typeName = typeof(TEvent).Name;
        var topic = typeName.EndsWith("Dto", StringComparison.OrdinalIgnoreCase)
            ? typeName.Substring(0, typeName.Length - 3).ToLower() 
            : typeName.ToLower();
            
        var value = JsonSerializer.Serialize(@event, JsonOptions);
        
        Console.WriteLine($"[Catalog.KafkaEventPublisher] Publicando evento no tópico '{topic}': {value}");
        
        await _producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = Guid.NewGuid().ToString(),
            Value = value
        });
        
        Console.WriteLine($"[Catalog.KafkaEventPublisher] Evento publicado com sucesso no tópico '{topic}'");
    }
}