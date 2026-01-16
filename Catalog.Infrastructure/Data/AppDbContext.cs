using Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;


namespace Catalog.Infrastructure.Data;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Id)
                .HasConversion(
                    id => id.Value,
                    value => new ProductId(value)
                );

            entity.OwnsOne(p => p.Valor, price =>
            {
                price.Property(p => p.Value).HasColumnName("price_amount");
            });

            entity.OwnsOne(typeof(Stock), "_stock", stock =>
            {
                stock.Property<int>("Quantity").HasColumnName("stock_quantity");
                
                stock.Ignore("Id");
                stock.Ignore("Movements");
                
                stock.OwnsMany(typeof(StockMovement), "_movements", movement =>
                {
                    movement.ToTable("stock_movements");
                    movement.HasKey("Id");
                    movement.Property<Guid>("Id").ValueGeneratedOnAdd();
                    movement.Property<int>("Quantity");
                    movement.Property<string>("InvoiceNumber").IsRequired();
                    movement.Property<DateTime>("Date");
                });
            });
            
            entity.Ignore("StockMovements");
        });
    }
}