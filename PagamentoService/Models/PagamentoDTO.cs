namespace PagamentoService.Models;

public class PagamentoConfirmado
{
    public int Id { get; set; } 
    public int PedidoId { get; set; }
    
    public int ClienteId { get; set; }

    public string StatusPagamento { get; set; } = "confirmado";

    public decimal ValorPago { get; set; }
    public DateTime DataConfirmacao { get; set; }
}
