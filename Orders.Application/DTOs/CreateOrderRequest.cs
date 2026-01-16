namespace Orders.Application.DTOs;

public class CreateOrderRequest
{
    public string DocumentoUsuario { get; set; }
    public string Vendedor { get; set; }
    public List<OrderItemRequest> Items { get; set; } = new();
}

public class OrderItemRequest
{
    public Guid ProductId { get; set; }
    public int Qtd { get; set; }
}