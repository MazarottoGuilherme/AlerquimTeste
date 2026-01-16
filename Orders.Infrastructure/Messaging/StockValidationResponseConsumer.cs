using System.Text.Json;
using System.Text.Json.Serialization;
using Orders.Application.Services;
using Orders.Domain.Events;
using Confluent.Kafka;

namespace Orders.Infrastructure.Messaging;

public class StockValidationResponseConsumer
{
    private readonly StockValidationResponseManager _responseManager;
    private readonly IConsumer<string, string> _consumer;

    public StockValidationResponseConsumer(string bootstrapServers, string topic, string groupId, StockValidationResponseManager responseManager)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _consumer.Subscribe(topic);

        _responseManager = responseManager;
    }

    public void StartConsuming(CancellationToken cancellationToken)
    {
        Task.Run(() =>
        {
            Console.WriteLine($"[StockValidationResponseConsumer] Iniciando consumo do tópico stockvalidationresponseevent...");
            
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(cancellationToken);
                    Console.WriteLine($"[StockValidationResponseConsumer] Mensagem recebida: {result.Message.Value}");

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var validationResponse = JsonSerializer.Deserialize<StockValidationResponseDto>(result.Message.Value, options);

                    if (validationResponse == null)
                    {
                        Console.WriteLine("[StockValidationResponseConsumer] ERRO: Falha ao deserializar evento - validationResponse é null");
                        continue;
                    }

                    Console.WriteLine($"[StockValidationResponseConsumer] Processando resposta de validação RequestId: {validationResponse.RequestId}, IsValid: {validationResponse.IsValid}");

                    var completed = _responseManager.TryCompleteValidation(
                        validationResponse.RequestId, 
                        validationResponse.IsValid, 
                        validationResponse.Message ?? string.Empty
                    );

                    if (!completed)
                    {
                        Console.WriteLine($"[StockValidationResponseConsumer] ATENÇÃO: Resposta recebida para RequestId inexistente: {validationResponse.RequestId}");
                    }
                }
                catch (ConsumeException ex)
                {
                    Console.WriteLine($"[StockValidationResponseConsumer] ERRO ao consumir mensagem: {ex.Error.Reason}");
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"[StockValidationResponseConsumer] ERRO ao deserializar JSON: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[StockValidationResponseConsumer] ERRO inesperado: {ex.Message}");
                    Console.WriteLine($"[StockValidationResponseConsumer] StackTrace: {ex.StackTrace}");
                }
            }
        }, cancellationToken);
    }

    private class StockValidationResponseDto
    {
        [JsonPropertyName("requestId")]
        public Guid RequestId { get; set; }
        
        [JsonPropertyName("isValid")]
        public bool IsValid { get; set; }
        
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
