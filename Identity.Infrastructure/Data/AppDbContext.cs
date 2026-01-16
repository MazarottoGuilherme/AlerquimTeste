using Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<UserDomain> Users { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserDomain>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.Id); 
            entity.Property(u => u.Id)
                .HasConversion(
                    id => id.Value, 
                    value => new UserId(value) 
                );
            entity.OwnsOne(u => u.Email);
        });
    }

}