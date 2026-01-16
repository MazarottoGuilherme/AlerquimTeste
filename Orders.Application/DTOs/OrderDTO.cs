namespace Orders.Application.DTOs;

public class OrderDTO
{
    public Guid Id { get; set; }
    public string DocumentoUsuario { get; set; }
    public string Vendedor { get; set; }
    public List<OrderItemDTO> Items { get; set; } = new();
}

public class OrderItemDTO
{
    public Guid ProductId { get; set; }
    public int Qtd { get; set; }
}
