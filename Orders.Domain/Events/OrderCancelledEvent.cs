namespace Orders.Domain.Events;

public class OrderCancelledEvent
{
    public Guid OrderId { get; }

    public OrderCancelledEvent(Guid orderId)
    {
        OrderId = orderId;
    }

}