using Catalog.Domain;
using Catalog.Domain.Products;

namespace Catalog.Tests.Domain;

public class StockTests
{
    [Fact]
    public void Empty_ShouldReturnStockWithZeroQuantity()
    {
        var stock = Stock.Empty();
        
        Assert.NotNull(stock);
        Assert.Equal(0, stock.Quantity);
        Assert.Empty(stock.Movements);
    }
    
    [Fact]
    public void Add_ValidQuantity_ShouldIncreaseQuantity()
    {
        var stock = Stock.Empty();
        
        stock.Add(10, "NF001");
        
        Assert.Equal(10, stock.Quantity);
        Assert.Single(stock.Movements);
        Assert.Equal("NF001", stock.Movements.First().InvoiceNumber);
    }
    
    [Fact]
    public void Add_MultipleAdditions_ShouldAccumulateQuantity()
    {
        var stock = Stock.Empty();
        
        stock.Add(5, "NF001");
        stock.Add(10, "NF002");
        stock.Add(3, "NF003");
        
        Assert.Equal(18, stock.Quantity);
        Assert.Equal(3, stock.Movements.Count);
    }
    
    [Fact]
    public void Remove_ValidQuantity_ShouldDecreaseQuantity()
    {
        var stock = Stock.Empty();
        stock.Add(10, "NF001");
        
        stock.Remove(4);
        
        Assert.Equal(6, stock.Quantity);
    }
    
    [Fact]
    public void Remove_QuantityEqualToStock_ShouldSetToZero()
    {
        var stock = Stock.Empty();
        stock.Add(10, "NF001");
        
        stock.Remove(10);
        
        Assert.Equal(0, stock.Quantity);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Remove_InvalidQuantity_ShouldThrowDomainException(int invalidQuantity)
    {
        var stock = Stock.Empty();
        stock.Add(10, "NF001");
        
        var ex = Assert.Throws<DomainException>(() => stock.Remove(invalidQuantity));
        Assert.Equal("Quantidade invalida", ex.Message);
    }
    
    [Fact]
    public void Remove_InsufficientStock_ShouldThrowDomainException()
    {
        var stock = Stock.Empty();
        stock.Add(10, "NF001");
        
        var ex = Assert.Throws<DomainException>(() => stock.Remove(15));
        Assert.Equal("Estoque insuficiente", ex.Message);
    }
    
    [Fact]
    public void Remove_AllStock_ThenRemove_ShouldThrowDomainException()
    {
        var stock = Stock.Empty();
        stock.Add(10, "NF001");
        stock.Remove(10);
        
        var ex = Assert.Throws<DomainException>(() => stock.Remove(1));
        Assert.Equal("Estoque insuficiente", ex.Message);
    }
}

