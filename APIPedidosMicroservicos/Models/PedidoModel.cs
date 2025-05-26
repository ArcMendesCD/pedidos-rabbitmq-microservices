using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class PedidoItemDTO
{
    public int ProdutoId { get; set; }
    public int Quantidade { get; set; }
}

public class PedidoDTO
{
    public int PedidoId { get; set; }
    public int ClienteId { get; set; }
    public List<PedidoItemDTO> Itens { get; set; }
    public decimal ValorTotal { get; set; }
}