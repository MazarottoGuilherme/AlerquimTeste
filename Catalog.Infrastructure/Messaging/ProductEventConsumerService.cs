using Catalog.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Catalog.Infrastructure.Messaging;

public class ProductEventConsumerService : BackgroundService
{
    private readonly IProductService _productService;
    private readonly IEventPublisher _eventPublisher;
    private readonly string _bootstrapServers;
    private readonly string _orderCreatedTopic = "ordercreatedevent";
    private readonly string _stockValidationTopic = "stockvalidationrequestevent";

    public ProductEventConsumerService(IProductService productService, IEventPublisher eventPublisher, IConfiguration configuration)
    {
        _productService = productService;
        _eventPublisher = eventPublisher;
        _bootstrapServers = configuration["Kafka:BootstrapServers"];
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrEmpty(_bootstrapServers))
        {
            Console.WriteLine("[ProductEventConsumerService] ERRO: Kafka BootstrapServers não configurado!");
            return Task.CompletedTask;
        }
        
        Console.WriteLine($"[ProductEventConsumerService] Iniciando consumers Kafka. BootstrapServers: {_bootstrapServers}");
        
        var orderCreatedConsumer = new KafkaConsumer(_bootstrapServers, _orderCreatedTopic, "catalog-group", _productService);
        orderCreatedConsumer.StartConsuming(stoppingToken);

        var stockValidationConsumer = new StockValidationConsumer(_bootstrapServers, _stockValidationTopic, "catalog-validation-group", _productService, _eventPublisher);
        stockValidationConsumer.StartConsuming(stoppingToken);
        
        return Task.CompletedTask;
    }
}

