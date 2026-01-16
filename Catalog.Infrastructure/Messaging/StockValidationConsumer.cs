using System.Text.Json;
using Catalog.Application.DTOs.Events;
using Catalog.Application.Interfaces;
using Confluent.Kafka;

namespace Catalog.Infrastructure.Messaging;

public class StockValidationConsumer
{
    private readonly IProductService _productService;
    private readonly IEventPublisher _eventPublisher;
    private readonly IConsumer<string, string> _consumer;

    public StockValidationConsumer(string bootstrapServers, string topic, string groupId, IProductService productService, IEventPublisher eventPublisher)
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
        _eventPublisher = eventPublisher;
    }

    public void StartConsuming(CancellationToken cancellationToken)
    {
        Task.Run(() =>
        {
            Console.WriteLine($"[StockValidationConsumer] Iniciando consumo do tópico stockvalidationrequestevent...");
            
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(cancellationToken);
                    Console.WriteLine($"[StockValidationConsumer] Mensagem recebida: {result.Message.Value}");

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var validationRequest = JsonSerializer.Deserialize<StockValidationRequestEventDto>(result.Message.Value, options);

                    if (validationRequest == null)
                    {
                        Console.WriteLine("[StockValidationConsumer] ERRO: Falha ao deserializar evento - validationRequest é null");
                        continue;
                    }

                    if (validationRequest.Items == null || !validationRequest.Items.Any())
                    {
                        Console.WriteLine("[StockValidationConsumer] ERRO: Evento não possui items válidos");
                        continue;
                    }

                    Console.WriteLine($"[StockValidationConsumer] Processando validação de estoque RequestId: {validationRequest.RequestId}, Items: {validationRequest.Items.Count}");

                    var isValid = _productService.ValidateStock(validationRequest.Items);
                    var message = isValid 
                        ? "Estoque disponível para todos os produtos" 
                        : "Estoque insuficiente para um ou mais produtos";

                    Console.WriteLine($"[StockValidationConsumer] Validação concluída para RequestId: {validationRequest.RequestId}, IsValid: {isValid}");

                    _productService.PublishStockValidationResponse(validationRequest.RequestId, isValid, message).Wait();
                }
                catch (ConsumeException ex)
                {
                    Console.WriteLine($"[StockValidationConsumer] ERRO ao consumir mensagem: {ex.Error.Reason}");
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"[StockValidationConsumer] ERRO ao deserializar JSON: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[StockValidationConsumer] ERRO inesperado: {ex.Message}");
                    Console.WriteLine($"[StockValidationConsumer] StackTrace: {ex.StackTrace}");
                }
            }
        }, cancellationToken);
    }
}
