namespace Catalog.Domain.Products;

public sealed class Money
{
    public decimal Value { get; }

    private Money(decimal value)
    {
        Value = value;
    }

    public static Money From(decimal value)
    {
        if (value <= 0)
            throw new DomainException("O valor deve ser maior que zero");

        return new Money(decimal.Round(value, 2));
    }
}
