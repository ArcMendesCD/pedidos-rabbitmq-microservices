using EstoqueService.Services;
using System.Text.Json.Serialization;

namespace EstoqueService.Models;

public class PedidoItemDTO
{
    [JsonConverter(typeof(Int32JsonConverter))]
    public int ProdutoId { get; set; }

    [JsonConverter(typeof(Int32JsonConverter))]
    public int Quantidade { get; set; }
}

public class PedidoDTO
{
    [JsonConverter(typeof(Int32JsonConverter))]
    public int PedidoId { get; set; }

    [JsonConverter(typeof(Int32JsonConverter))]
    public int ClienteId { get; set; }

    public List<PedidoItemDTO> Itens { get; set; }

    public decimal ValorTotal { get; set; }
}