using NotificacaoService.Services;

namespace NotificacaoService.Models;
using System.Text.Json.Serialization;


public class PagamentoConfirmadoDTO
{
    [JsonConverter(typeof(Int32JsonConverter))]
    public int PedidoId { get; set; }
    [JsonConverter(typeof(Int32JsonConverter))]

    public int ClienteId { get; set; }

    public decimal ValorPago { get; set; }

    public string StatusPagamento { get; set; }

    public DateTime DataConfirmacao { get; set; }
}