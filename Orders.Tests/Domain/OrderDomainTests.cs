using Orders.Domain;
using Orders.Domain.Orders;

namespace Orders.Tests.Domain;

public class OrderDomainTests
{
    [Fact]
    public void CreateOrder_ValidData_ShouldSetProperties()
    {
        var userDocument = "123456789";
        var salesPerson = "Exemplo de teste";

        var order = Order.Create(userDocument, salesPerson);

        Assert.NotEqual(Guid.Empty, order.Id.Value);
        Assert.Equal(userDocument, order.DocumentoUsuario);
        Assert.Equal(salesPerson, order.Vendedor);
        Assert.Empty(order.Items);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateOrder_EmptyUserDocument_ShouldThrowDomainException(string invalidDocument)
    {
        var ex = Assert.Throws<DomainException>(() => Order.Create(invalidDocument, "Exemplo de teste"));
        Assert.Equal("Documento do usuario obrigatorio", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateOrder_EmptySalesPerson_ShouldThrowDomainException(string invalidSalesPerson)
    {
        var ex = Assert.Throws<DomainException>(() => Order.Create("123", invalidSalesPerson));
        Assert.Equal("Vendedor é obrigatorio", ex.Message);
    }

    [Fact]
    public void AddItem_ValidItem_ShouldAddToOrder()
    {
        var order = Order.Create("123456789", "Exemplo de teste");
        var item = new OrderItem(Guid.NewGuid(), 2);

        order.AddItem(item);

        Assert.Single(order.Items);
        Assert.Contains(item, order.Items);
    }

    [Fact]
    public void AddItem_MultipleItems_ShouldAddAllItems()
    {
        var order = Order.Create("123456789", "Exemplo de teste");
        var item1 = new OrderItem(Guid.NewGuid(), 2);
        var item2 = new OrderItem(Guid.NewGuid(), 3);

        order.AddItem(item1);
        order.AddItem(item2);

        Assert.Equal(2, order.Items.Count);
        Assert.Contains(item1, order.Items);
        Assert.Contains(item2, order.Items);
    }

    [Fact]
    public void AddItem_NullItem_ShouldThrowDomainException()
    {
        var order = Order.Create("123456789", "Exemplo de teste");

        var ex = Assert.Throws<DomainException>(() => order.AddItem(null));
        Assert.Equal("Item não pode ser null", ex.Message);
    }

    [Fact]
    public void AddItem_DuplicateProduct_ShouldThrowDomainException()
    {
        var order = Order.Create("123456789", "Exemplo de teste");
        var productId = Guid.NewGuid();
        var item1 = new OrderItem(productId, 1);
        var item2 = new OrderItem(productId, 2);

        order.AddItem(item1);

        var ex = Assert.Throws<DomainException>(() => order.AddItem(item2));
        Assert.Equal("Produto já adicionado ao pedido", ex.Message);
    }

    [Fact]
    public void MarkAsCancelled_ShouldChangeStatusToCancelled()
    {
        var order = Order.Create("123456789", "Exemplo de teste");

        order.MarkAsCancelled();

        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void MarkAsCancelled_AlreadyCancelled_ShouldThrowDomainException()
    {
        var order = Order.Create("123456789", "Exemplo de teste");
        order.MarkAsCancelled();

        var ex = Assert.Throws<DomainException>(() => order.MarkAsCancelled());
        Assert.Equal("Pedido já está cancelado", ex.Message);
    }

    [Fact]
    public void MarkAsCompleted_ShouldChangeStatusToCompleted()
    {
        var order = Order.Create("123456789", "Exemplo de teste");

        order.MarkAsCompleted();

        Assert.Equal(OrderStatus.Completed, order.Status);
    }

    [Fact]
    public void OrderItem_NegativeQuantity_ShouldThrowDomainException()
    {
        var productId = Guid.NewGuid();
        var ex = Assert.Throws<DomainException>(() => new OrderItem(productId, 0));
        Assert.Equal("Quantidade deve ser maior que zero", ex.Message);
    }

    [Fact]
    public void OrderItem_ValidQuantity_ShouldCreateItem()
    {
        var productId = Guid.NewGuid();
        var quantity = 5;

        var item = new OrderItem(productId, quantity);

        Assert.Equal(productId, item.ProductId);
        Assert.Equal(quantity, item.Quantity);
    }
}
