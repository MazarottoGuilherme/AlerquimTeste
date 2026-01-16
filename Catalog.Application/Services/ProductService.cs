using Catalog.Application.DTOs;
using Catalog.Application.DTOs.Events;
using Catalog.Application.Interfaces;
using Catalog.Domain;
using Catalog.Domain.Products;

namespace Catalog.Application.Services;

public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        private readonly IEventPublisher _eventPublisher;

        public ProductService(IProductRepository repo, IEventPublisher eventPublisher)
        {
            _repo = repo;
            _eventPublisher = eventPublisher;
        }

        public ProductDTO CreateProduct(CreateProductRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nome))
                throw new DomainException("Nome é obrigatorio");

            if (string.IsNullOrWhiteSpace(request.Descricao))
                throw new DomainException("Descricao é obrigatorio");

            if (request.Valor <= 0)
                throw new DomainException("Valor deve ser maior que zero");

            var money = Money.From(request.Valor);
            var product = Product.Create(request.Nome, request.Descricao, money);

            _repo.Add(product);

            return new ProductDTO()
            {
                Id = product.Id.Value,
                Nome = product.Nome,
                Descricao = product.Descricao,
                Valor = product.Valor.Value,
                EstoqueQTD = product.StockQuantity
            };
        }

        public void AddStock(Guid productId, int quantity, string invoiceNumber)
        {
            var product = _repo.GetById(new ProductId(productId));
            if (product == null)
                throw new DomainException("Produto não encontrado");

            product.AddStock(quantity, invoiceNumber);
            _repo.Update(product);
        }

        public ProductDTO GetProductById(Guid productId)
        {
            var product = _repo.GetById(new ProductId(productId));
            if (product == null)
                throw new DomainException("Produto não encontrado");

            return new ProductDTO
            {
                Id = product.Id.Value,
                Nome = product.Nome,
                Descricao = product.Descricao,
                Valor = product.Valor.Value,
                EstoqueQTD = product.StockQuantity
            };
        }

        public ProductDTO UpdateProduct(Guid productId, UpdateProductRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nome))
                throw new DomainException("Nome é obrigatorio");

            if (string.IsNullOrWhiteSpace(request.Descricao))
                throw new DomainException("Descricao é obrigatorio");

            if (request.Valor <= 0)
                throw new DomainException("Valor deve ser maior que zero");

            var product = _repo.GetById(new ProductId(productId));
            if (product == null)
                throw new DomainException("Produto não encontrado");

            var money = Money.From(request.Valor);
            product.Update(request.Nome, request.Descricao, money);
            _repo.Update(product);

            return new ProductDTO
            {
                Id = product.Id.Value,
                Nome = product.Nome,
                Descricao = product.Descricao,
                Valor = product.Valor.Value,
                EstoqueQTD = product.StockQuantity
            };
        }

        public void DeleteProduct(Guid productId)
        {
            var product = _repo.GetById(new ProductId(productId));
            if (product == null)
                throw new DomainException("Produto não encontrado");

            _repo.Delete(product);
        }

        public IEnumerable<Product> GetAll()
        {
            return _repo.GetAll();
        }


        public bool DecreaseStock(List<OrderItemDto> items)
        {
            Console.WriteLine($"[ProductService] DecreaseStock chamado para {items.Count} item(s)");
            
            foreach (var item in items)
            {
                var product = _repo.GetById(new ProductId(item.ProductId));
                if (product == null)
                {
                    Console.WriteLine($"[ProductService] Produto {item.ProductId} não encontrado");
                    return false;
                }
                
                if (product.StockQuantity < item.Quantity)
                {
                    Console.WriteLine($"[ProductService] Estoque insuficiente. Produto {item.ProductId}: estoque atual {product.StockQuantity}, necessário {item.Quantity}");
                    return false;
                }
            }

            foreach (var item in items)
            {
                var product = _repo.GetById(new ProductId(item.ProductId));
                var estoqueAnterior = product.StockQuantity;
                product.RemoveStock(item.Quantity);
                _repo.Update(product);
                Console.WriteLine($"[ProductService] Estoque do produto {item.ProductId} atualizado: {estoqueAnterior} -> {product.StockQuantity}");
            }

            return true;
        }
        
        public async Task PublishOrderCancelled(Guid orderId)
        {
            var cancelEvent = new OrderCancelledEventDto
            {
                OrderId = orderId
            };

            await _eventPublisher.PublishAsync(cancelEvent);
        }
        
        public bool ValidateStock(List<OrderItemDto> items)
        {
            Console.WriteLine($"[ProductService] ValidateStock chamado para {items.Count} item(s)");
            
            foreach (var item in items)
            {
                var product = _repo.GetById(new ProductId(item.ProductId));
                if (product == null)
                {
                    Console.WriteLine($"[ProductService] Produto {item.ProductId} não encontrado");
                    return false;
                }
                
                if (!product.HasStock(item.Quantity))
                {
                    Console.WriteLine($"[ProductService] Estoque insuficiente. Produto {item.ProductId}: estoque atual {product.StockQuantity}, necessário {item.Quantity}");
                    return false;
                }
            }

            Console.WriteLine($"[ProductService] Validação de estoque bem-sucedida para {items.Count} item(s)");
            return true;
        }

        public async Task PublishStockValidationResponse(Guid requestId, bool isValid, string message)
        {
            var responseEvent = new StockValidationResponseEventDto
            {
                RequestId = requestId,
                IsValid = isValid,
                Message = message
            };

            await _eventPublisher.PublishAsync(responseEvent);
        }
    }
