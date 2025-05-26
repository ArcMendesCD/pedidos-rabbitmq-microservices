namespace PagamentoService.Entities;

public class Pagamento
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public string StatusPagamento { get; set; } = "confirmado";

    public int ClienteId { get; set; }
    public decimal Valor { get; set; }
    public DateTime DataPagamento { get; set; } = DateTime.UtcNow;
}
