using Catalog.Application.DTOs;
using Catalog.Application.DTOs.Events;
using Catalog.Application.Interfaces;
using Catalog.Application.Services;
using Catalog.Domain;
using Catalog.Domain.Products;
using Moq;

namespace Catalog.Tests.Application;

public class ProductApplicationTests
{
    private readonly Mock<IProductRepository> _repoMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly ProductService _service;

    public ProductApplicationTests()
    {
        _repoMock = new Mock<IProductRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _service = new ProductService(_repoMock.Object, _eventPublisherMock.Object);
    }

    [Fact]
    public void CreateProduct_ValidRequest_ShouldAddProductAndReturnDTO()
    {
        var request = new CreateProductRequest
        {
            Nome = "Guitarra Elétrica",
            Descricao = "Guitarra elétrica profissional com captadores humbucker",
            Valor = 100m
        };

        Product capturedProduct = null;
        _repoMock.Setup(r => r.Add(It.IsAny<Product>()))
                 .Callback<Product>(p => capturedProduct = p);

        var dto = _service.CreateProduct(request);

        _repoMock.Verify(r => r.Add(It.IsAny<Product>()), Times.Once);
        Assert.NotNull(dto);
        Assert.Equal(request.Nome, dto.Nome);
        Assert.Equal(request.Descricao, dto.Descricao);
        Assert.Equal(request.Valor, dto.Valor);
        Assert.NotNull(capturedProduct);
        Assert.Equal(capturedProduct.StockQuantity, dto.EstoqueQTD);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateProduct_InvalidName_ShouldThrowDomainException(string invalidName)
    {
        var request = new CreateProductRequest
        {
            Nome = invalidName,
            Descricao = "Violão acústico de 6 cordas",
            Valor = 50m
        };

        var ex = Assert.Throws<DomainException>(() => _service.CreateProduct(request));
        Assert.Equal("Nome é obrigatorio", ex.Message);
        
        _repoMock.Verify(r => r.Add(It.IsAny<Product>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateProduct_InvalidDescription_ShouldThrowDomainException(string invalidDescription)
    {
        var request = new CreateProductRequest
        {
            Nome = "Violão Acústico",
            Descricao = invalidDescription,
            Valor = 50m
        };

        var ex = Assert.Throws<DomainException>(() => _service.CreateProduct(request));
        Assert.Equal("Descricao é obrigatorio", ex.Message);
        
        _repoMock.Verify(r => r.Add(It.IsAny<Product>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void CreateProduct_InvalidPrice_ShouldThrowDomainException(decimal invalidPrice)
    {
        var request = new CreateProductRequest
        {
            Nome = "Violão Acústico",
            Descricao = "Violão acústico de 6 cordas",
            Valor = invalidPrice
        };

        var ex = Assert.Throws<DomainException>(() => _service.CreateProduct(request));
        Assert.Equal("Valor deve ser maior que zero", ex.Message);
        
        _repoMock.Verify(r => r.Add(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public void AddStock_ProductExists_ShouldAddStock()
    {
        var productId = Guid.NewGuid();
        var product = Product.Create("Baixo Elétrico", "Baixo elétrico de 4 cordas", Money.From(50m));

        _repoMock.Setup(r => r.GetById(It.Is<ProductId>(id => id.Value == productId)))
                 .Returns(product);

        _service.AddStock(productId, 10, "NF001");

        Assert.Equal(10, product.StockQuantity);
        _repoMock.Verify(r => r.GetById(It.Is<ProductId>(id => id.Value == productId)), Times.Once);
        _repoMock.Verify(r => r.Update(product), Times.Once);
    }

    [Fact]
    public void AddStock_ProductNotFound_ShouldThrowDomainException()
    {
        var productId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetById(It.Is<ProductId>(id => id.Value == productId)))
                 .Returns((Product)null);

        var ex = Assert.Throws<DomainException>(() => _service.AddStock(productId, 5, "NF001"));
        Assert.Equal("Produto não encontrado", ex.Message);
        
        _repoMock.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public void GetAll_ShouldReturnAllProducts()
    {
        var products = new List<Product>
        {
            Product.Create("Guitarra Elétrica", "Guitarra elétrica profissional", Money.From(1000m)),
            Product.Create("Bateria Acústica", "Bateria acústica completa 5 peças", Money.From(2000m))
        };

        _repoMock.Setup(r => r.GetAll()).Returns(products);

        var result = _service.GetAll();

        Assert.Equal(2, result.Count());
        _repoMock.Verify(r => r.GetAll(), Times.Once);
    }

    [Fact]
    public void GetAll_NoProducts_ShouldReturnEmpty()
    {
        _repoMock.Setup(r => r.GetAll()).Returns(new List<Product>());

        var result = _service.GetAll();

        Assert.Empty(result);
    }

    [Fact]
    public void DecreaseStock_AllProductsHaveStock_ShouldReturnTrue()
    {
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        
        var product1 = Product.Create("Guitarra Elétrica", "Guitarra elétrica profissional", Money.From(1000m));
        product1.AddStock(10, "NF001");
        
        var product2 = Product.Create("Bateria Acústica", "Bateria acústica completa", Money.From(2000m));
        product2.AddStock(5, "NF002");

        var items = new List<OrderItemDto>
        {
            new OrderItemDto { ProductId = productId1, Quantity = 5 },
            new OrderItemDto { ProductId = productId2, Quantity = 3 }
        };

        _repoMock.Setup(r => r.GetById(It.Is<ProductId>(id => id.Value == productId1)))
                 .Returns(product1);
        _repoMock.Setup(r => r.GetById(It.Is<ProductId>(id => id.Value == productId2)))
                 .Returns(product2);

        var result = _service.DecreaseStock(items);

        Assert.True(result);
        Assert.Equal(5, product1.StockQuantity);
        Assert.Equal(2, product2.StockQuantity);
        _repoMock.Verify(r => r.Update(product1), Times.Once);
        _repoMock.Verify(r => r.Update(product2), Times.Once);
    }

    [Fact]
    public void DecreaseStock_ProductNotFound_ShouldReturnFalse()
    {
        var productId = Guid.NewGuid();
        var items = new List<OrderItemDto>
        {
            new OrderItemDto { ProductId = productId, Quantity = 5 }
        };

        _repoMock.Setup(r => r.GetById(It.Is<ProductId>(id => id.Value == productId)))
                 .Returns((Product)null);

        var result = _service.DecreaseStock(items);

        Assert.False(result);
        _repoMock.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public void DecreaseStock_InsufficientStock_ShouldReturnFalse()
    {
        var productId = Guid.NewGuid();
        var product = Product.Create("Teclado Digital", "Teclado digital com 88 teclas", Money.From(1500m));
        product.AddStock(5, "NF001");

        var items = new List<OrderItemDto>
        {
            new OrderItemDto { ProductId = productId, Quantity = 10 }
        };

        _repoMock.Setup(r => r.GetById(It.Is<ProductId>(id => id.Value == productId)))
                 .Returns(product);

        var result = _service.DecreaseStock(items);

        Assert.False(result);
        Assert.Equal(5, product.StockQuantity); 
        _repoMock.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public void DecreaseStock_MultipleItemsWithInsufficientStock_ShouldReturnFalse()
    {
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        
        var product1 = Product.Create("Violão Acústico", "Violão acústico de 6 cordas", Money.From(800m));
        product1.AddStock(10, "NF001");
        
        var product2 = Product.Create("Microfone Condensador", "Microfone condensador para estúdio", Money.From(500m));
        product2.AddStock(2, "NF002"); // Insuficiente

        var items = new List<OrderItemDto>
        {
            new OrderItemDto { ProductId = productId1, Quantity = 5 },
            new OrderItemDto { ProductId = productId2, Quantity = 5 } 
        };

        _repoMock.Setup(r => r.GetById(It.Is<ProductId>(id => id.Value == productId1)))
                 .Returns(product1);
        _repoMock.Setup(r => r.GetById(It.Is<ProductId>(id => id.Value == productId2)))
                 .Returns(product2);

        var result = _service.DecreaseStock(items);

        Assert.False(result);
        Assert.Equal(10, product1.StockQuantity);
        Assert.Equal(2, product2.StockQuantity); 
        _repoMock.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task PublishOrderCancelled_ShouldPublishEvent()
    {
        var orderId = Guid.NewGuid();

        await _service.PublishOrderCancelled(orderId);

        _eventPublisherMock.Verify(
            e => e.PublishAsync(It.Is<OrderCancelledEventDto>(evt => evt.OrderId == orderId)),
            Times.Once);
    }

    [Fact]
    public void GetProductById_ProductExists_ShouldReturnDTO()
    {
        var productId = Guid.NewGuid();
        var product = Product.Create("Guitarra Elétrica", "Guitarra elétrica profissional", Money.From(1000m));
        product.AddStock(5, "NF001");

        _repoMock.Setup(r => r.GetById(It.Is<ProductId>(id => id.Value == productId)))
                 .Returns(product);

        var result = _service.GetProductById(productId);

        Assert.NotNull(result);
        Assert.Equal(product.Id.Value, result.Id);
        Assert.Equal(product.Nome, result.Nome);
        Assert.Equal(product.Descricao, result.Descricao);
        Assert.Equal(product.Valor.Value, result.Valor);
        Assert.Equal(product.StockQuantity, result.EstoqueQTD);
    }

    [Fact]
    public void GetProductById_ProductNotFound_ShouldThrowDomainException()
    {
        var productId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetById(It.Is<ProductId>(id => id.Value == productId)))
                 .Returns((Product)null);

        var ex = Assert.Throws<DomainException>(() => _service.GetProductById(productId));
        Assert.Equal("Produto não encontrado", ex.Message);
    }

    [Fact]
    public void UpdateProduct_ProductExists_ShouldUpdateAndReturnDTO()
    {
        var productId = Guid.NewGuid();
        var product = Product.Create("Guitarra Elétrica", "Guitarra elétrica profissional", Money.From(1000m));
        product.AddStock(5, "NF001");

        var request = new UpdateProductRequest
        {
            Nome = "Violão Acústico",
            Descricao = "Violão acústico de 6 cordas",
            Valor = 800m
        };

        _repoMock.Setup(r => r.GetById(It.Is<ProductId>(id => id.Value == productId)))
                 .Returns(product);

        var result = _service.UpdateProduct(productId, request);

        Assert.Equal(request.Nome, result.Nome);
        Assert.Equal(request.Descricao, result.Descricao);
        Assert.Equal(request.Valor, result.Valor);
        Assert.Equal(5, result.EstoqueQTD); 
        _repoMock.Verify(r => r.Update(product), Times.Once);
    }

    [Fact]
    public void UpdateProduct_ProductNotFound_ShouldThrowDomainException()
    {
        var productId = Guid.NewGuid();
        var request = new UpdateProductRequest
        {
            Nome = "Violão Acústico",
            Descricao = "Violão acústico de 6 cordas",
            Valor = 800m
        };

        _repoMock.Setup(r => r.GetById(It.Is<ProductId>(id => id.Value == productId)))
                 .Returns((Product)null);

        var ex = Assert.Throws<DomainException>(() => _service.UpdateProduct(productId, request));
        Assert.Equal("Produto não encontrado", ex.Message);

        _repoMock.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateProduct_InvalidName_ShouldThrowDomainException(string invalidName)
    {
        var productId = Guid.NewGuid();
        var product = Product.Create("Guitarra Elétrica", "Guitarra elétrica profissional", Money.From(1000m));

        var request = new UpdateProductRequest
        {
            Nome = invalidName,
            Descricao = "Violão acústico de 6 cordas",
            Valor = 800m
        };

        _repoMock.Setup(r => r.GetById(It.Is<ProductId>(id => id.Value == productId)))
                 .Returns(product);

        var ex = Assert.Throws<DomainException>(() => _service.UpdateProduct(productId, request));
        Assert.Equal("Nome é obrigatorio", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void UpdateProduct_InvalidPrice_ShouldThrowDomainException(decimal invalidPrice)
    {
        var productId = Guid.NewGuid();
        var product = Product.Create("Guitarra Elétrica", "Guitarra elétrica profissional", Money.From(1000m));

        var request = new UpdateProductRequest
        {
            Nome = "Violão Acústico",
            Descricao = "Violão acústico de 6 cordas",
            Valor = invalidPrice
        };

        _repoMock.Setup(r => r.GetById(It.Is<ProductId>(id => id.Value == productId)))
                 .Returns(product);

        var ex = Assert.Throws<DomainException>(() => _service.UpdateProduct(productId, request));
        Assert.Equal("Valor deve ser maior que zero", ex.Message);
    }

    [Fact]
    public void DeleteProduct_ProductExists_ShouldDelete()
    {
        var productId = Guid.NewGuid();
        var product = Product.Create("Guitarra Elétrica", "Guitarra elétrica profissional", Money.From(1000m));

        _repoMock.Setup(r => r.GetById(It.Is<ProductId>(id => id.Value == productId)))
                 .Returns(product);

        _service.DeleteProduct(productId);

        _repoMock.Verify(r => r.Delete(product), Times.Once);
    }

    [Fact]
    public void DeleteProduct_ProductNotFound_ShouldThrowDomainException()
    {
        var productId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetById(It.Is<ProductId>(id => id.Value == productId)))
                 .Returns((Product)null);

        var ex = Assert.Throws<DomainException>(() => _service.DeleteProduct(productId));
        Assert.Equal("Produto não encontrado", ex.Message);

        _repoMock.Verify(r => r.Delete(It.IsAny<Product>()), Times.Never);
    }
}
