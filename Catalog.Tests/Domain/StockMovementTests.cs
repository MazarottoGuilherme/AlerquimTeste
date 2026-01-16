using Catalog.Domain;
using Catalog.Domain.Products;

namespace Catalog.Tests.Domain;

public class StockMovementTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void StockMovement_InvalidQuantity_ShouldThrowDomainException(int invalidQuantity)
    {
        var ex = Assert.Throws<DomainException>(() => new StockMovement(invalidQuantity, "NF001"));
        Assert.Equal("Quantidade invalida", ex.Message);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void StockMovement_InvalidInvoiceNumber_ShouldThrowDomainException(string invalidInvoice)
    {
        var ex = Assert.Throws<DomainException>(() => new StockMovement(10, invalidInvoice));
        Assert.Equal("Nota fiscal é obrigatorio", ex.Message);
    }
}

