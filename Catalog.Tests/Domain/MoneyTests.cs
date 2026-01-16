using Catalog.Domain;
using Catalog.Domain.Products;

namespace Catalog.Tests.Domain;

public class MoneyTests
{
    [Fact]
    public void From_ValidValue_ShouldCreateMoney()
    {
        var value = 100.50m;
        
        var money = Money.From(value);
        
        Assert.NotNull(money);
        Assert.Equal(value, money.Value);
    }
    
    [Fact]
    public void From_ValueWithMoreThanTwoDecimals_ShouldRoundToTwoDecimals()
    {
        var value = 100.999m;
        
        var money = Money.From(value);
        
        Assert.Equal(101.00m, money.Value);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void From_InvalidValue_ShouldThrowDomainException(decimal invalidValue)
    {
        var ex = Assert.Throws<DomainException>(() => Money.From(invalidValue));
        Assert.Equal("O valor deve ser maior que zero", ex.Message);
    }
    
    [Fact]
    public void From_LargeValue_ShouldCreateMoney()
    {
        var value = 999999.99m;
        
        var money = Money.From(value);
        
        Assert.Equal(value, money.Value);
    }
}

