using Orders.Domain.Orders;

namespace Orders.Application.Interfaces;

public interface IOrderRepository
{
    void Add(Order order);
    Order GetById(OrderId id);
    IEnumerable<Order> GetAll();
    void Update(Order order); 

}