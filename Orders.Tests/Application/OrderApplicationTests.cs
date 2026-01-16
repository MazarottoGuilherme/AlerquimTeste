using Moq;
using Orders.Application.DTOs;
using Orders.Application.Interfaces;
using Orders.Application.Services;
using Orders.Domain;
using Orders.Domain.Events;
using Orders.Domain.Orders;

namespace Orders.Tests.Application;

public class OrderApplicationTests
{
    private readonly Mock<IOrderRepository> _repoMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly StockValidationResponseManager _validationResponseManager;
    private readonly OrderService _service;

    public OrderApplicationTests()
    {
        _repoMock = new Mock<IOrderRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _validationResponseManager = new StockValidationResponseManager();
        _service = new OrderService(_repoMock.Object, _eventPublisherMock.Object, _validationResponseManager);
    }

    [Fact]
    public async Task CreateOrderAsync_ValidRequest_ShouldCallRepositoryAndReturnDTO()
    {
        var productId = Guid.NewGuid();
        var request = new CreateOrderRequest
        {
            DocumentoUsuario = "123456789",
            Vendedor = "Exemplo de teste",
            Items = new List<OrderItemRequest>
            {
                new() { ProductId = productId, Qtd = 2 }
            }
        };

        Guid capturedRequestId = Guid.Empty;
        _eventPublisherMock
            .Setup(e => e.PublishAsync(It.IsAny<StockValidationRequestEvent>()))
            .Returns<StockValidationRequestEvent>(evt =>
            {
                capturedRequestId = evt.RequestId;
                _validationResponseManager.TryCompleteValidation(capturedRequestId, true, "Estoque disponível");
                return Task.CompletedTask; 
            });


        var result = await _service.CreateOrderAsync(request);

        _repoMock.Verify(r => r.Add(It.IsAny<Order>()), Times.Once);
        _eventPublisherMock.Verify(
            e => e.PublishAsync(It.IsAny<StockValidationRequestEvent>()),
            Times.Once);
        _eventPublisherMock.Verify(
            e => e.PublishAsync(It.IsAny<OrderCreatedEvent>()),
            Times.Once);
        
        Assert.NotNull(result);
        Assert.Equal(request.DocumentoUsuario, result.DocumentoUsuario);
        Assert.Equal(request.Vendedor, result.Vendedor);
        Assert.Single(result.Items);
        Assert.Equal(request.Items[0].Qtd, result.Items[0].Qtd);
    }

    [Fact]
    public async Task CreateOrderAsync_MultipleItems_ShouldCreateOrderWithAllItems()
    {
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        var request = new CreateOrderRequest
        {
            DocumentoUsuario = "123456789",
            Vendedor = "Exemplo de teste",
            Items = new List<OrderItemRequest>
            {
                new() { ProductId = productId1, Qtd = 2 },
                new() { ProductId = productId2, Qtd = 3 }
            }
        };

        _eventPublisherMock
            .Setup(e => e.PublishAsync(It.IsAny<StockValidationRequestEvent>()))
            .Returns<StockValidationRequestEvent>(evt =>
            {
                _validationResponseManager.TryCompleteValidation(evt.RequestId, true, "Estoque disponível");
                return Task.CompletedTask;
            });

        var result = await _service.CreateOrderAsync(request);

        Assert.Equal(2, result.Items.Count);
        _repoMock.Verify(r => r.Add(It.IsAny<Order>()), Times.Once);
        _eventPublisherMock.Verify(
            e => e.PublishAsync(It.Is<OrderCreatedEvent>(evt => evt.Items.Count == 2)),
            Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateOrderAsync_EmptyUserDocument_ShouldThrowDomainException(string invalidDocument)
    {
        var request = new CreateOrderRequest
        {
            DocumentoUsuario = invalidDocument,
            Vendedor = "Exemplo de teste",
            Items = new List<OrderItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), Qtd = 2 }
            }
        };

        var ex = await Assert.ThrowsAsync<DomainException>(() => _service.CreateOrderAsync(request));
        Assert.Equal("Documento do usuario é obrigatorio", ex.Message);

        _repoMock.Verify(r => r.Add(It.IsAny<Order>()), Times.Never);
        _eventPublisherMock.Verify(e => e.PublishAsync(It.IsAny<OrderCreatedEvent>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrderAsync_StockValidationFailed_ShouldThrowDomainException()
    {
        var productId = Guid.NewGuid();
        var request = new CreateOrderRequest
        {
            DocumentoUsuario = "123456789",
            Vendedor = "Exemplo de teste",
            Items = new List<OrderItemRequest>
            {
                new() { ProductId = productId, Qtd = 2 }
            }
        };

        _eventPublisherMock
            .Setup(e => e.PublishAsync(It.IsAny<StockValidationRequestEvent>()))
            .Returns<StockValidationRequestEvent>(async evt =>
            {
                await Task.Delay(10); 
                _validationResponseManager.TryCompleteValidation(
                    evt.RequestId, 
                    false, 
                    "Estoque insuficiente para um ou mais produtos"
                );
            });

        var ex = await Assert.ThrowsAsync<DomainException>(() => _service.CreateOrderAsync(request));
        Assert.Contains("Validação de estoque falhou", ex.Message);

        _repoMock.Verify(r => r.Add(It.IsAny<Order>()), Times.Never);
        _eventPublisherMock.Verify(e => e.PublishAsync(It.IsAny<OrderCreatedEvent>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrderAsync_StockValidationTimeout_ShouldThrowDomainException()
    {
        var productId = Guid.NewGuid();
        var request = new CreateOrderRequest
        {
            DocumentoUsuario = "123456789",
            Vendedor = "Exemplo de teste",
            Items = new List<OrderItemRequest>
            {
                new() { ProductId = productId, Qtd = 10 }
            }
        };

        var ex = await Assert.ThrowsAsync<DomainException>(() => _service.CreateOrderAsync(request));
        Assert.Contains("Timeout ao aguardar validação de estoque", ex.Message);

        _repoMock.Verify(r => r.Add(It.IsAny<Order>()), Times.Never);
        _eventPublisherMock.Verify(e => e.PublishAsync(It.IsAny<OrderCreatedEvent>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateOrderAsync_EmptyVendedor_ShouldThrowDomainException(string invalidVendedor)
    {
        var request = new CreateOrderRequest
        {
            DocumentoUsuario = "123456789",
            Vendedor = invalidVendedor,
            Items = new List<OrderItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), Qtd = 2 }
            }
        };

        var ex = await Assert.ThrowsAsync<DomainException>(() => _service.CreateOrderAsync(request));
        Assert.Equal("Vendedor é obrigatorio", ex.Message);

        _repoMock.Verify(r => r.Add(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrderAsync_NoItems_ShouldThrowDomainException()
    {
        var request = new CreateOrderRequest
        {
            DocumentoUsuario = "123",
            Vendedor = "Exemplo de teste",
            Items = new List<OrderItemRequest>()
        };

        var ex = await Assert.ThrowsAsync<DomainException>(() => _service.CreateOrderAsync(request));
        Assert.Equal("Pedido deve ter no minimo um produto", ex.Message);

        _repoMock.Verify(r => r.Add(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrderAsync_NullItems_ShouldThrowDomainException()
    {
        var request = new CreateOrderRequest
        {
            DocumentoUsuario = "123",
            Vendedor = "Exemplo de teste",
            Items = null
        };

        var ex = await Assert.ThrowsAsync<DomainException>(() => _service.CreateOrderAsync(request));
        Assert.Equal("Pedido deve ter no minimo um produto", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task CreateOrderAsync_ItemWithZeroOrNegativeQuantity_ShouldThrowDomainException(int invalidQtd)
    {
        var request = new CreateOrderRequest
        {
            DocumentoUsuario = "123",
            Vendedor = "Exemplo de teste",
            Items = new List<OrderItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), Qtd = invalidQtd }
            }
        };

        var ex = await Assert.ThrowsAsync<DomainException>(() => _service.CreateOrderAsync(request));
        Assert.Equal("Quantidade deve ser maior que zero", ex.Message);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldPublishOrderCreatedEventWithCorrectData()
    {
        var productId = Guid.NewGuid();
        var request = new CreateOrderRequest
        {
            DocumentoUsuario = "123456789",
            Vendedor = "Exemplo de teste",
            Items = new List<OrderItemRequest>
            {
                new() { ProductId = productId, Qtd = 5 }
            }
        };

        _eventPublisherMock
            .Setup(e => e.PublishAsync(It.IsAny<StockValidationRequestEvent>()))
            .Returns<StockValidationRequestEvent>(async evt =>
            {
                await Task.Delay(10);
                _validationResponseManager.TryCompleteValidation(
                    evt.RequestId,
                    true,
                    "Estoque disponível"
                );
            });

        await _service.CreateOrderAsync(request);

        _eventPublisherMock.Verify(
            e => e.PublishAsync(It.Is<OrderCreatedEvent>(evt =>
                evt.Items.Count == 1 &&
                evt.Items[0].ProductId == productId &&
                evt.Items[0].Quantity == 5 &&
                evt.CustomerDocument == "123456789" &&
                evt.SellerName == "Exemplo de teste")),
            Times.Once);
    }

    [Fact]
    public void GetOrderById_OrderExists_ShouldReturnDTO()
    {
        var orderId = OrderId.New();
        var order = Order.Create("123", "Exemplo de teste");
        order.AddItem(new OrderItem(Guid.NewGuid(), 3));

        _repoMock.Setup(r => r.GetById(orderId)).Returns(order);

        var result = _service.GetOrderById(orderId);

        Assert.NotNull(result);
        Assert.Equal(order.Id.Value, result.Id);
        Assert.Equal(order.DocumentoUsuario, result.DocumentoUsuario);
        Assert.Equal(order.Vendedor, result.Vendedor);
        Assert.Single(result.Items);
        Assert.Equal(3, result.Items.First().Qtd);
    }

    [Fact]
    public void GetOrderById_OrderNotFound_ShouldThrowDomainException()
    {
        var orderId = OrderId.New();
        _repoMock.Setup(r => r.GetById(orderId)).Returns((Order)null);

        var ex = Assert.Throws<DomainException>(() => _service.GetOrderById(orderId));
        Assert.Equal("Pedido nao encontrado", ex.Message);
    }

    [Fact]
    public void GetAllOrders_ShouldReturnAllOrders()
    {
        var orders = new List<Order>
        {
            Order.Create("123", "Exemplo de teste"),
            Order.Create("456", "Exemplo de teste")
        };

        _repoMock.Setup(r => r.GetAll()).Returns(orders);

        var result = _service.GetAllOrders();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task CancelOrderAsync_OrderExists_ShouldMarkAsCancelledAndPublishEvent()
    {
        var orderId = OrderId.New();
        var order = Order.Create("123", "Exemplo de teste");

        _repoMock.Setup(r => r.GetById(orderId)).Returns(order);

        await _service.CancelOrderAsync(orderId);

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        _repoMock.Verify(r => r.Update(order), Times.Once);
        _eventPublisherMock.Verify(
            e => e.PublishAsync(It.Is<OrderCancelledEvent>(evt => evt.OrderId == order.Id.Value)),
            Times.Once);
    }

    [Fact]
    public async Task CancelOrderAsync_OrderNotFound_ShouldNotThrowException()
    {
        var orderId = OrderId.New();
        _repoMock.Setup(r => r.GetById(orderId)).Returns((Order)null);

        await _service.CancelOrderAsync(orderId);

        _repoMock.Verify(r => r.Update(It.IsAny<Order>()), Times.Never);
        _eventPublisherMock.Verify(e => e.PublishAsync(It.IsAny<OrderCancelledEvent>()), Times.Never);
    }
}
