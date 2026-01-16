namespace Orders.Domain.Orders;

public class Order
{
    public OrderId Id { get; }
    public string DocumentoUsuario { get; private set; }
    public string Vendedor { get; private set; }

    private readonly List<OrderItem> _items = new();
    
    public OrderStatus Status { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items;

    private Order() { } 
    
    private Order(OrderId id, string userDocument, string salesPerson)
    {
        if (string.IsNullOrWhiteSpace(userDocument))
            throw new DomainException("Documento do usuario obrigatorio");

        if (string.IsNullOrWhiteSpace(salesPerson))
            throw new DomainException("Vendedor é obrigatorio");

        Id = id;
        DocumentoUsuario = userDocument;
        Vendedor = salesPerson;
    }

    
    public static Order Create(string userDocument, string salesPerson)
    {
        return new Order(OrderId.New(), userDocument, salesPerson);
    }
    
    public void AddItem(OrderItem item)
    {
        if (item == null)
            throw new DomainException("Item não pode ser null");

        if (_items.Any(i => i.ProductId == item.ProductId))
            throw new DomainException("Produto já adicionado ao pedido");

        _items.Add(item);
    }
    
    public void MarkAsCancelled()
    {
        if (Status == OrderStatus.Cancelled)
            throw new DomainException("Pedido já está cancelado");

        Status = OrderStatus.Cancelled;
    }

    public void MarkAsCompleted()
    {
        Status = OrderStatus.Completed;
    }


}