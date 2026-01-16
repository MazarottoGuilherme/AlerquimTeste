namespace Orders.Domain.Orders;

public class OrderItem
{
    public Guid ProductId { get; }
    public int Quantity { get; private set; }

    private OrderItem() { }

    public OrderItem(Guid productId, int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantidade deve ser maior que zero");

        ProductId = productId;
        Quantity = quantity;
    }

}