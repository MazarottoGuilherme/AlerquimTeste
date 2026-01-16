using System.Text.Json.Serialization;

namespace Catalog.Application.DTOs.Events;

public class OrderCreatedEventDto
{
    [JsonPropertyName("orderId")]
    public Guid OrderId { get; set; }
    
    [JsonPropertyName("customerDocument")]
    public string CustomerDocument { get; set; }
    
    [JsonPropertyName("sellerName")]
    public string SellerName { get; set; }
    
    [JsonPropertyName("items")]
    public List<OrderItemDto> Items { get; set; }
}

public class OrderItemDto
{
    [JsonPropertyName("productId")]
    public Guid ProductId { get; set; }
    
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
}
