using Catalog.Application.DTOs;
using Catalog.Application.DTOs.Events;
using Catalog.Domain.Products;

namespace Catalog.Application.Interfaces;

public interface IProductService
{
    ProductDTO CreateProduct(CreateProductRequest request);
    ProductDTO GetProductById(Guid productId);
    ProductDTO UpdateProduct(Guid productId, UpdateProductRequest request);
    void DeleteProduct(Guid productId);
    void AddStock(Guid productId, int quantity, string invoiceNumber);
    IEnumerable<Product> GetAll();
    bool DecreaseStock(List<OrderItemDto> items);
    Task PublishOrderCancelled(Guid orderId);
    bool ValidateStock(List<OrderItemDto> items);
    Task PublishStockValidationResponse(Guid requestId, bool isValid, string message);
}