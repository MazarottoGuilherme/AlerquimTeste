namespace Catalog.Application.DTOs;

public class CreateProductRequest
{
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public decimal Valor { get; set; }
}