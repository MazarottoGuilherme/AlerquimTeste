using System.Text.Json;
using Catalog.Application.DTOs.Events;
using Catalog.Application.Interfaces;
using Confluent.Kafka;

namespace Catalog.Infrastructure.Messaging;

public class KafkaConsumer
{
    private readonly IProductService _productService;
    private readonly IConsumer<string, string> _consumer;

    public KafkaConsumer(string bootstrapServers, string topic, string groupId, IProductService productService)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _consumer.Subscribe(topic);

        _productService = productService;
    }

    public void StartConsuming(CancellationToken cancellationToken)
    {
        Task.Run(() =>
        {
            Console.WriteLine($"[KafkaConsumer] Iniciando consumo do tópico...");
            
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(cancellationToken);
                    Console.WriteLine($"[KafkaConsumer] Mensagem recebida: {result.Message.Value}");

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var orderEvent = JsonSerializer.Deserialize<OrderCreatedEventDto>(result.Message.Value, options);

                    if (orderEvent == null)
                    {
                        Console.WriteLine("[KafkaConsumer] ERRO: Falha ao deserializar evento - orderEvent é null");
                        continue;
                    }

                    if (orderEvent.Items == null || !orderEvent.Items.Any())
                    {
                        Console.WriteLine("[KafkaConsumer] ERRO: Evento não possui items válidos");
                        continue;
                    }

                    Console.WriteLine($"[KafkaConsumer] Processando evento OrderId: {orderEvent.OrderId}, Items: {orderEvent.Items.Count}");

                    var success = _productService.DecreaseStock(orderEvent.Items);

                    if (!success)
                    {
                        Console.WriteLine($"[KafkaConsumer] Estoque insuficiente para o pedido {orderEvent.OrderId}. Cancelando pedido...");
                        _productService.PublishOrderCancelled(orderEvent.OrderId).Wait();
                    }
                    else
                    {
                        Console.WriteLine($"[KafkaConsumer] Estoque atualizado com sucesso para o pedido {orderEvent.OrderId}");
                    }
                }
                catch (ConsumeException ex)
                {
                    Console.WriteLine($"[KafkaConsumer] ERRO ao consumir mensagem: {ex.Error.Reason}");
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"[KafkaConsumer] ERRO ao deserializar JSON: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[KafkaConsumer] ERRO inesperado: {ex.Message}");
                    Console.WriteLine($"[KafkaConsumer] StackTrace: {ex.StackTrace}");
                }
            }
        }, cancellationToken);
    }
}

