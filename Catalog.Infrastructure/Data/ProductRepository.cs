using Catalog.Application.Interfaces;
using Catalog.Domain.Products;

namespace Catalog.Infrastructure.Data;

public class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _context;

    public ProductRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public void Add(Product product)
    {
        _context.Products.Add(product);
        _context.SaveChanges();
    }
    
    public void Update(Product product)
    {
        _context.Products.Update(product);
        _context.SaveChanges();
    }


    public Product GetById(ProductId id)
    {
        return _context.Products
            .FirstOrDefault(p => p.Id == id);
    }

    public IEnumerable<Product> GetAll()
    {
        return _context.Products
            .ToList();
    }

    public void Delete(Product product)
    {
        _context.Products.Remove(product);
        _context.SaveChanges();
    }
}

