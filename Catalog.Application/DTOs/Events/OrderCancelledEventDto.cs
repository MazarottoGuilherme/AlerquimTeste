namespace Catalog.Application.DTOs.Events;

public class OrderCancelledEventDto
{
    public Guid OrderId { get; set; }
    public DateTime CancelledAt { get; set; } = DateTime.UtcNow;
}