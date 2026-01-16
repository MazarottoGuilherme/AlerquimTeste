namespace Orders.Domain.Events;

public class StockValidationRequestEvent
{
    public Guid RequestId { get; }
    public List<OrderItemDto> Items { get; }

    public StockValidationRequestEvent(Guid requestId, List<OrderItemDto> items)
    {
        RequestId = requestId;
        Items = items;
    }
}
