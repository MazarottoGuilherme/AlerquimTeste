using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Orders.Application.Services;

namespace Orders.Infrastructure.Messaging;

public class OrderEventConsumerService : BackgroundService
{
    private readonly StockValidationResponseManager _responseManager;
    private readonly string _bootstrapServers;
    private readonly string _stockValidationResponseTopic = "stockvalidationresponseevent";

    public OrderEventConsumerService(StockValidationResponseManager responseManager, IConfiguration configuration)
    {
        _responseManager = responseManager;
        _bootstrapServers = configuration["Kafka:BootstrapServers"];
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrEmpty(_bootstrapServers))
        {
            Console.WriteLine("[OrderEventConsumerService] ERRO: Kafka BootstrapServers não configurado!");
            return Task.CompletedTask;
        }
        
        Console.WriteLine($"[OrderEventConsumerService] Iniciando consumer Kafka. BootstrapServers: {_bootstrapServers}, Topic: {_stockValidationResponseTopic}");
        
        var consumer = new StockValidationResponseConsumer(_bootstrapServers, _stockValidationResponseTopic, "orders-group", _responseManager);
        consumer.StartConsuming(stoppingToken);
        
        return Task.CompletedTask;
    }
}
