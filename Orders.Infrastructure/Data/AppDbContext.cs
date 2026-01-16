using Orders.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Orders.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Id)
                .HasConversion(
                    id => id.Value,
                    value => new OrderId(value)
                );
            entity.Property(o => o.DocumentoUsuario).IsRequired();
            entity.Property(o => o.Vendedor).IsRequired();

            entity.OwnsMany(o => o.Items, item =>
            {
                item.ToTable("order_items");
                item.WithOwner().HasForeignKey("OrderId");
                item.HasKey("OrderId", "ProductId");
                item.Property(i => i.ProductId);
                item.Property(i => i.Quantity);
            });
        });
    }
}

