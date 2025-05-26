namespace EstoqueService.Models;

public class EstoqueReservadoDTO
{
    public string PedidoId { get; set; }
    public string StatusEstoque { get; set; }
    public List<ReservaItemDTO> ItensReservados { get; set; }
    public DateTime DataReserva { get; set; }
}

public class ReservaItemDTO
{
    public string ProdutoId { get; set; }
    public int Quantidade { get; set; }
}