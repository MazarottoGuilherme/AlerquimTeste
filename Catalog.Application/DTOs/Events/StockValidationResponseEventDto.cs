using System.Text.Json.Serialization;

namespace Catalog.Application.DTOs.Events;

public class StockValidationResponseEventDto
{
    [JsonPropertyName("requestId")]
    public Guid RequestId { get; set; }
    
    [JsonPropertyName("isValid")]
    public bool IsValid { get; set; }
    
    [JsonPropertyName("message")]
    public string Message { get; set; }
}
