using Catalog.Domain;
using Catalog.Domain.Products;

namespace Catalog.Tests.Domain;

public class ProductDomainTests
{
    [Fact]
    public void CreateProduct_ValidInput_ShouldReturnProduct()
    {
        var name = "Guitarra Elétrica";
        var description = "Guitarra elétrica profissional com captadores humbucker";
        var price = Money.From(100m);

        var product = Product.Create(name, description, price);

        Assert.NotNull(product);
        Assert.Equal(name, product.Nome);
        Assert.Equal(description, product.Descricao);
        Assert.Equal(price.Value, product.Valor.Value);
        Assert.Equal(0, product.StockQuantity);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateProduct_InvalidName_ShouldThrowDomainException(string invalidName)
    {
        var description = "Descrição";
        var price = Money.From(50m);

        var ex = Assert.Throws<DomainException>(() => Product.Create(invalidName, description, price));
        Assert.Equal("Nome é obrigatorio", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateProduct_InvalidDescription_ShouldThrowDomainException(string invalidDescription)
    {
        var name = "Baixo Elétrico";
        var price = Money.From(50m);

        var ex = Assert.Throws<DomainException>(() => Product.Create(name, invalidDescription, price));
        Assert.Equal("Descrição é obrigatorio", ex.Message);
    }

    [Fact]
    public void CreateProduct_NullPrice_ShouldThrowDomainException()
    {
        var name = "Teclado Digital";
        var description = "Teclado digital com 88 teclas";

        var ex = Assert.Throws<DomainException>(() => Product.Create(name, description, null));
        Assert.Equal("Valor é obrigatorio", ex.Message);
    }

    [Fact]
    public void AddStock_ValidQuantity_ShouldIncreaseStock()
    {
        var product = Product.Create("Violão Acústico", "Violão acústico de 6 cordas", Money.From(50m));

        product.AddStock(10, "NF123");

        Assert.Equal(10, product.StockQuantity);
        Assert.Single(product.StockMovements);
        Assert.Equal("NF123", Assert.Single(product.StockMovements).InvoiceNumber);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void AddStock_InvalidQuantity_ShouldThrowDomainException(int quantity)
    {
        var product = Product.Create("Violão Acústico", "Violão acústico de 6 cordas", Money.From(50m));

        var ex = Assert.Throws<DomainException>(() => product.AddStock(quantity, "NF123"));
        Assert.Equal("Valor deve ser maior que zero", ex.Message);
    }

    [Fact]
    public void AddStock_EmptyInvoice_ShouldThrowDomainException()
    {
        var product = Product.Create("Violão Acústico", "Violão acústico de 6 cordas", Money.From(50m));

        var ex = Assert.Throws<DomainException>(() => product.AddStock(5, ""));
        Assert.Equal("Nota fiscal é obrigatorio", ex.Message);
    }

    [Fact]
    public void RemoveStock_ValidQuantity_ShouldDecreaseStock()
    {
        var product = Product.Create("Violão Acústico", "Violão acústico de 6 cordas", Money.From(50m));
        product.AddStock(10, "NF001");

        product.RemoveStock(4);

        Assert.Equal(6, product.StockQuantity);
    }
    
    [Fact]
    public void HasStock_ShouldReturnTrueOrFalse()
    {
        var product = Product.Create("Violão Acústico", "Violão acústico de 6 cordas", Money.From(50m));
        product.AddStock(10, "NF001");

        Assert.True(product.HasStock(5));
        Assert.True(product.HasStock(10));
        Assert.False(product.HasStock(15));
    }
}