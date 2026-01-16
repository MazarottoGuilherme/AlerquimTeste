namespace Catalog.Application.DTOs;

public class ProductDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public decimal Valor { get; set; }
    public int EstoqueQTD { get; set; }

}