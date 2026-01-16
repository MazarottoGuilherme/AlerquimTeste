namespace Catalog.Domain.Products;

public class Stock
{
    public Guid Id { get; private set; } 
    public int Quantity { get; private set; }

    private readonly List<StockMovement> _movements = new();
    public IReadOnlyCollection<StockMovement> Movements => _movements;

    private Stock() { }

    private Stock(int quantity)
    {
        Quantity = quantity;
    }

    public static Stock Empty() => new Stock(0);

    public void Add(int quantity, string invoice)
    {
        var movement = new StockMovement(quantity, invoice);

        Quantity += movement.Quantity;
        _movements.Add(movement);
    }


    public void Remove(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantidade invalida");

        if (Quantity < quantity)
            throw new DomainException("Estoque insuficiente");

        Quantity -= quantity;
    }
}