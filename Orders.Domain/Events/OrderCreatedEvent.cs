namespace Orders.Domain.Events;

public class OrderCreatedEvent
{
    public Guid OrderId { get; }
    public List<OrderItemDto> Items { get; }
    public string CustomerDocument { get; }
    public string SellerName { get; }

    public OrderCreatedEvent(Guid orderId, List<OrderItemDto> items, string customerDocument, string sellerName)
    {
        OrderId = orderId;
        Items = items;
        CustomerDocument = customerDocument;
        SellerName = sellerName;
    }

}
public class OrderItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
