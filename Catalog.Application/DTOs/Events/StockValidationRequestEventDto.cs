using System.Text.Json.Serialization;

namespace Catalog.Application.DTOs.Events;

public class StockValidationRequestEventDto
{
    [JsonPropertyName("requestId")]
    public Guid RequestId { get; set; }
    
    [JsonPropertyName("items")]
    public List<OrderItemDto> Items { get; set; }
}
