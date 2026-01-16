using Catalog.Domain.Products;

namespace Catalog.Application.Interfaces;

public interface IProductRepository
{
    void Add(Product product);
    Product GetById(ProductId id);
    IEnumerable<Product> GetAll();
    void Update(Product product);
    void Delete(Product product);

}