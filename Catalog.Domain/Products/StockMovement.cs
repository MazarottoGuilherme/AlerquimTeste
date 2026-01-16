namespace Catalog.Domain.Products;

public class StockMovement
{
    public Guid Id { get; private set; } 
    public int Quantity { get; }
    public string InvoiceNumber { get; }
    public DateTime Date { get; }

    private StockMovement() { }

    public StockMovement(int quantity, string invoiceNumber)
    {
        if (quantity <= 0)
            throw new DomainException("Quantidade invalida");

        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new DomainException("Nota fiscal é obrigatorio");

        Quantity = quantity;
        InvoiceNumber = invoiceNumber;
        Date = DateTime.UtcNow;
    }
}