namespace Catalog.Domain.Products;

public class Product
{
    public ProductId Id { get; private set; }
    public string Nome { get; private set; }
    public string Descricao { get; private set; }
    public Money Valor { get; private set; }
    
    private Stock _stock;

    public int StockQuantity => _stock.Quantity;
    public IReadOnlyCollection<StockMovement> StockMovements => _stock.Movements;


    private Product() { }
    
    private Product(ProductId id, string name, string description, Money price)
    {
        Id = id;
        Nome = name;
        Descricao = description;
        Valor = price;
        _stock = Stock.Empty();
        
    }
    
    public static Product Create(string name, string description, Money price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome é obrigatorio");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Descrição é obrigatorio");

        if (price is null)
            throw new DomainException("Valor é obrigatorio");

        return new Product(ProductId.New(), name, description, price);
    }

    public void AddStock(int quantity, string nf)
    {
        if(quantity <= 0)
            throw new DomainException("Valor deve ser maior que zero");
        if (string.IsNullOrWhiteSpace(nf))
            throw new DomainException("Nota fiscal é obrigatorio");

        _stock.Add(quantity, nf);
    }

    public void RemoveStock(int qtd)
    {
        _stock.Remove(qtd);
    }

    public bool HasStock(int qtd)
    {
        return _stock.Quantity >= qtd;
    }

    public void Update(string name, string description, Money price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome é obrigatorio");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Descrição é obrigatorio");

        if (price is null)
            throw new DomainException("Valor é obrigatorio");

        Nome = name;
        Descricao = description;
        Valor = price;
    }
}
