using Orders.Application.DTOs;
using Orders.Application.Interfaces;
using Orders.Domain;
using Orders.Domain.Events;
using Orders.Domain.Orders;

namespace Orders.Application.Services;

public class OrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IEventPublisher _eventPublisher;
        private readonly StockValidationResponseManager _validationResponseManager;
        private const int ValidationTimeoutSeconds = 10;

        public OrderService(IOrderRepository orderRepo, IEventPublisher eventPublisher, StockValidationResponseManager validationResponseManager)
        {
            _orderRepo = orderRepo;
            _eventPublisher = eventPublisher;
            _validationResponseManager = validationResponseManager;
        }

        public async Task<OrderDTO> CreateOrderAsync(CreateOrderRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.DocumentoUsuario))
                throw new DomainException("Documento do usuario é obrigatorio");

            if (string.IsNullOrWhiteSpace(request.Vendedor))
                throw new DomainException("Vendedor é obrigatorio");

            if (request.Items == null || !request.Items.Any())
                throw new DomainException("Pedido deve ter no minimo um produto");

            foreach (var itemReq in request.Items)
            {
                if (itemReq.Qtd <= 0)
                    throw new DomainException("Quantidade deve ser maior que zero");
            }

            var requestId = Guid.NewGuid();
            var itemsForValidation = request.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                Quantity = i.Qtd
            }).ToList();

            Console.WriteLine($"[OrderService] Publicando evento de validação de estoque RequestId: {requestId}, Items: {itemsForValidation.Count}");

            var validationTask = _validationResponseManager.CreatePendingValidation(requestId);

            var validationRequestEvent = new StockValidationRequestEvent(requestId, itemsForValidation);
            await _eventPublisher.PublishAsync(validationRequestEvent);

            var validationResult = await Task.WhenAny(
                validationTask.Task,
                Task.Delay(TimeSpan.FromSeconds(ValidationTimeoutSeconds))
            );

            if (validationResult == validationTask.Task)
            {
                var (isValid, message) = await validationTask.Task;
                if (!isValid)
                {
                    Console.WriteLine($"[OrderService] Validação de estoque falhou: {message}");
                    throw new DomainException($"Validação de estoque falhou: {message}");
                }
                Console.WriteLine($"[OrderService] Validação de estoque bem-sucedida: {message}");
            }
            else
            {
                _validationResponseManager.CancelPendingValidation(requestId);
                Console.WriteLine($"[OrderService] Timeout aguardando validação de estoque");
                throw new DomainException("Timeout ao aguardar validação de estoque. Tente novamente.");
            }

            var order = Order.Create(request.DocumentoUsuario, request.Vendedor);

            foreach (var itemReq in request.Items)
            {
                order.AddItem(new OrderItem(itemReq.ProductId, itemReq.Qtd));
            }

            _orderRepo.Add(order);

            var orderEvent = new OrderCreatedEvent(
                order.Id.Value,
                order.Items.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList(),
                order.DocumentoUsuario,
                order.Vendedor
            );

            await _eventPublisher.PublishAsync(orderEvent);

            return new OrderDTO
            {
                Id = order.Id.Value,
                DocumentoUsuario = order.DocumentoUsuario,
                Vendedor = order.Vendedor,
                Items = order.Items.Select(i => new OrderItemDTO
                {
                    ProductId = i.ProductId,
                    Qtd = i.Quantity
                }).ToList()
            };
        }

        public OrderDTO GetOrderById(OrderId id)
        {
            var order = _orderRepo.GetById(id);
            if (order == null)
                throw new DomainException("Pedido nao encontrado");

            return new OrderDTO
            {
                Id = order.Id.Value,
                DocumentoUsuario = order.DocumentoUsuario,
                Vendedor = order.Vendedor,
                Items = order.Items.Select(i => new OrderItemDTO
                {
                    ProductId = i.ProductId,
                    Qtd = i.Quantity
                }).ToList()
            };
        }

        public IEnumerable<Order> GetAllOrders()
        {
            return _orderRepo.GetAll();
        }

        public async Task CancelOrderAsync(OrderId id)
        {
            var order = _orderRepo.GetById(id);
            if (order == null) return;

            order.MarkAsCancelled();
            _orderRepo.Update(order);

            var cancelEvent = new OrderCancelledEvent(order.Id.Value);
            await _eventPublisher.PublishAsync(cancelEvent);
        }
    }
