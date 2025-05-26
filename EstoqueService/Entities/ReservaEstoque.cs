using EstoqueService.Models;

namespace EstoqueService.Entities;

public class ReservaEstoque
{
    public int Id { get; set; }
    public string PedidoId { get; set; }
    public List<ReservaItem> Itens { get; set; }
    public string StatusEstoque { get; set; }
    public DateTime DataReserva { get; set; }
}

public class ReservaItem
{
    public int Id { get; set; }
    public string ProdutoId { get; set; }
    public int Quantidade { get; set; }
}