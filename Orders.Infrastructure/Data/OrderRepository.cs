using Orders.Application.Interfaces;
using Orders.Domain.Orders;

namespace Orders.Infrastructure.Data;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(Order order)
    {
        _context.Orders.Add(order);
        _context.SaveChanges();
    }

    public Order GetById(OrderId id)
    {
        return _context.Orders
            .FirstOrDefault(o => o.Id == id);
    }

    public IEnumerable<Order> GetAll()
    {
        return _context.Orders
            .ToList();
    }
    
    public void Update(Order order)
    {
        _context.Orders.Update(order);
        _context.SaveChanges();
    }
}

